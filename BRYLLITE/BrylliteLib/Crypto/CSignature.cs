using BrylliteLib.Crypto.Secp256k1;
using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Asn1.X9;
using Org.BouncyCastle.Crypto.Signers;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Math.EC;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace BrylliteLib.Crypto
{
    public class CSignature
    {
        public static readonly int SIGNATURE_BYTES = 72;

        public BigInteger R { get; }
        public BigInteger S { get; }

        public byte V = 0xff;

        public bool IsLowS => S.CompareTo(ECKey.HALF_CURVE_ORDER) <= 0;

        public CSignature(BigInteger r, BigInteger s)
        {
            R = r;
            S = s;
        }

        public CSignature(BigInteger r, BigInteger s, byte v) : this(r, s)
        {
            V = v;
        }


        public CSignature(BigInteger[] rs) : this(rs[0], rs[1])
        {
        }

        public CSignature(byte[] der)
        {
            try
            {
                V = der[0];
                var decoder = new Asn1InputStream(der.Skip(1).ToArray());
                var seq = decoder.ReadObject() as DerSequence;
                if (seq == null || seq.Count != 2)
                    throw new FormatException("Invalid DER Signature");

                R = ((DerInteger)seq[0]).Value;
                S = ((DerInteger)seq[1]).Value;
            }
            catch (Exception e)
            {
                throw new FormatException("Invalid DER Signature", e);
            }
        }

        public CSignature MakeCanonical()
        {
            return IsLowS ? this : new CSignature(R, ECKey.CURVE_ORDER.Subtract(S), V);
        }

        // fixed 72 bytes
        public byte[] ToByteArray()
        {
            byte[] bytes = new byte[SIGNATURE_BYTES];
            bytes[0] = V;

            var memoryStream = new MemoryStream();
            var seq = new DerSequenceGenerator(memoryStream);
            seq.AddObject(new DerInteger(R));
            seq.AddObject(new DerInteger(S));
            seq.Close();

            byte[] der = memoryStream.ToArray();
            Debug.Assert(der.Length < SIGNATURE_BYTES);

            Buffer.BlockCopy(der, 0, bytes, 1, der.Length);
            return bytes;
        }

        public static CSignature FromByteArray(byte[] bytes)
        {
            byte V = bytes[0];

            try
            {
                var decoder = new Asn1InputStream(bytes.Skip(1).ToArray());
                var seq = decoder.ReadObject() as DerSequence;
                if (seq == null || seq.Count != 2)
                    throw new FormatException("Invalid DER Signature");

                return new CSignature(((DerInteger)seq[0]).Value, ((DerInteger)seq[1]).Value, V);
            }
            catch (Exception e)
            {
                throw new FormatException("Invalid DER Signature", e);
            }
        }

        public CPublicKey ToPublicKey(byte[] hash)
        {
            return ToPublicKey(hash, V);
        }

        internal CPublicKey ToPublicKey(byte[] hash, byte recoveryParameter)
        {
            if (recoveryParameter < 0) throw new ArgumentException("recoveryParameter should be positive", "recoverId");
            if (R.SignValue < 0) throw new ArgumentException("R should be positive", "R");
            if (S.SignValue < 0) throw new ArgumentException("S should be positive", "S");
            if (hash == null || hash.Length <= 0) throw new ArgumentException("message should be non-null", "message");

            var secp256k1 = ECKey.secp256k1;

            var n = secp256k1.N;
            var i = BigInteger.ValueOf((long)recoveryParameter / 2);
            var x = R.Add(i.Multiply(n));

            var prime = new BigInteger(1, Org.BouncyCastle.Utilities.Encoders.Hex.Decode(ECKey.SECP256K1_PRIME));
            if (x.CompareTo(prime) >= 0)
                return null;

            var r = DecompressKey(x, (recoveryParameter & 1) == 1);
            if (!r.Multiply(n).IsInfinity)
                return null;

            var e = new BigInteger(1, hash);

            var eInv = BigInteger.Zero.Subtract(e).Mod(n);
            var rInv = R.ModInverse(n);
            var srInv = rInv.Multiply(S).Mod(n);
            var eInvrInv = rInv.Multiply(eInv).Mod(n);
            var q = ECAlgorithms.SumOfTwoMultiplies(secp256k1.G, eInvrInv, r, srInv).Normalize();

            q = secp256k1.Curve.CreatePoint(q.XCoord.ToBigInteger(), q.YCoord.ToBigInteger());
            return new CPublicKey(q.GetEncoded(true));
        }

        private static ECPoint DecompressKey(BigInteger x, bool yBit)
        {
            var curve = ECKey.secp256k1.Curve;

            List<byte> listBytes = new List<byte>();
            listBytes.Add((byte)(yBit ? 0x03 : 0x02));
            listBytes.AddRange(X9IntegerConverter.IntegerToBytes(x, X9IntegerConverter.GetByteLength(curve)));

            return curve.DecodePoint(listBytes.ToArray());
        }

        public bool Verify(byte[] hash)
        {
            if (hash == null || hash.Length != 32) throw new ArgumentException("hash length should be 32 bytes", "hash");

            try
            {
                ECDsaSigner signer = new ECDsaSigner();
                signer.Init(false, ToPublicKey(hash).Key);

                return signer.VerifySignature(hash, R, S);
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static implicit operator byte[] (CSignature sig)
        {
            return sig.ToByteArray();
        }

        public static implicit operator CSignature(byte[] bytes)
        {
            return CSignature.FromByteArray(bytes);
        }
    }
}
