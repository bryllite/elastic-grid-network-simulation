using BrylliteLib.Crypto.Secp256k1;
using BrylliteLib.Hash;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Crypto.Signers;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Math.EC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BrylliteLib.Crypto
{
    public class CPublicKey : ECKey
    {
        public static readonly byte ADDRESS_PREFIX = 0xbc;
        public static readonly int ADDRESS_BYTES = CAddress.ADDRESS_BYTES;

        private static readonly BigInteger PRIME = new BigInteger(1, Org.BouncyCastle.Utilities.Encoders.Hex.Decode(SECP256K1_PRIME));

        public ECPublicKeyParameters Key => mKey as ECPublicKeyParameters;

        // 공개 키 No Prefix ( 64 bytes )
        public byte[] PublicKey
        {
            get
            {
                return GetPublicKey(false).Skip(1).ToArray();
            }
        }

        public CAddress Address
        {
            get
            {
                return Sha3.Hash256(PublicKey).Skip(12).ToArray();
            }
        }

        public string HexAddress
        {
            get
            {
                return Address.HexAddress;
            }
        }

        public byte[] GetPublicKey(bool compressed)
        {
            ECPoint q = Key.Q.Normalize();
            return secp256k1.Curve.CreatePoint(q.XCoord.ToBigInteger(), q.YCoord.ToBigInteger()).GetEncoded(compressed);
        }

        public CPublicKey(byte[] bytesPublicKey) : base(bytesPublicKey, false)
        {
        }

        public bool Verify(CSignature sig, byte[] hash)
        {
            if (hash == null || hash.Length != 32) throw new ArgumentException("hash length should be 32 bytes", "hash");

            try
            {
                ECDsaSigner signer = new ECDsaSigner();
                signer.Init(false, Key);

                return signer.VerifySignature(hash, sig.R, sig.S);
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
