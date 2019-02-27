using BrylliteLib.Crypto.Secp256k1;
using BrylliteLib.Hash;
using BrylliteLib.Utils;
using Newtonsoft.Json.Linq;
using Org.BouncyCastle.Asn1.Sec;
using Org.BouncyCastle.Asn1.X9;
using Org.BouncyCastle.Crypto.Digests;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Crypto.Signers;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Math.EC;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace BrylliteLib.Crypto
{
    public class CPrivateKey : ECKey
    {
        public static readonly int CHAIN_CODE_BYTES = 32;

        public ECPrivateKeyParameters Key => mKey as ECPrivateKeyParameters;

        // 체인 코드
        private byte[] mChainCode;
        public byte[] ChainCode
        {
            get
            {
                return mChainCode != null ? mChainCode : HashUtil.Hash256(PrivateKey);
            }
        }

        // 개인 키
        public byte[] PrivateKey
        {
            get
            {
                return Key.D.ToByteArrayUnsigned();
            }
        }

        // 개인 키에서 공개 키로
        public CPublicKey PublicKey
        {
            get
            {
                ECPoint q = secp256k1.G.Multiply(Key.D).Normalize();
                return new CPublicKey(secp256k1.Curve.CreatePoint(q.XCoord.ToBigInteger(), q.YCoord.ToBigInteger()).GetEncoded(true));
            }
        }

        // 공개 키에서 주소로
        public string HexAddress
        {
            get
            {
                return PublicKey.HexAddress;
            }
        }

        public CAddress Address
        {
            get
            {
                return PublicKey.Address;
            }
        }

        public string ToJsonString()
        {
            JObject j = new JObject();

            j.Add("PrivateKey", PrivateKey.ToHexString());
            j.Add("PublicKey", PublicKey.PublicKey.ToHexString());
            j.Add("ChainCode", ChainCode.ToHexString());
            j.Add("Address", HexAddress);

            return j.ToString();
        }

        public CPrivateKey(byte[] bytesPrivateKey) : base(bytesPrivateKey, true)
        {
        }

        public CPrivateKey(byte[] bytesPrivateKey, byte[] bytesChainCode) : base(bytesPrivateKey, true)
        {
            mChainCode = bytesChainCode;
        }

        public byte[] ToByteArray()
        {
            List<byte> bytes = new List<byte>();
            bytes.AddRange(PrivateKey);
            bytes.AddRange(ChainCode);

            return bytes.ToArray();
        }

        public CPrivateKey CKD(string keyPath)
        {
            return CKD(keyPath.Split('/'));
        }

        public CPrivateKey CKD(string[] keyPath)
        {
            if (keyPath == null || keyPath.Length == 0) throw new ArgumentNullException("keyPath");

            CPrivateKey childKey = this;
            for (int i = 0; i < keyPath.Length; i++)
                childKey = childKey.CKD(Encoding.UTF8.GetBytes(keyPath[i]));

            return childKey;
        }

        public CPrivateKey CKD(byte[] hexKeyPath)
        {
            return ToChildKey(hexKeyPath);
        }

        private CPrivateKey ToChildKey(byte[] bytesKeyPath)
        {
            byte[] childPrivateKey;
            byte[] childChainCode;
            byte nonce = 0;

            do
            {
                List<byte> byteList = new List<byte>();

                byteList.AddRange(PrivateKey);      // or PublicKey
                byteList.AddRange(bytesKeyPath);
                byteList.AddRange(ChainCode);
                byteList.Add(nonce++);

                byte[] bytesPrivateKeyAndChainCode = HashUtil.Hash512(byteList.ToArray());
                childPrivateKey = bytesPrivateKeyAndChainCode.Left();
                childChainCode = bytesPrivateKeyAndChainCode.Right();
            } while (!IsValidPrivateKey(childPrivateKey));

            return new CPrivateKey(childPrivateKey, childChainCode);
        }

        public static CPrivateKey CreateKey()
        {
            return new CPrivateKey(NewPrivateKeyAsBytes());
        }

        public static CPrivateKey CreateKeyFromPassPhrase(string passPhrase)
        {
            byte[] bytesPrivateKey;
            byte[] bytesChainCode;
            byte nonce = 0x00;

            do
            {
                string seed = passPhrase + "//" + nonce++.ToString("D2");

                byte[] bytesPrivateKeyAndChainCode = HashUtil.Hash512(seed);
                bytesPrivateKey = bytesPrivateKeyAndChainCode.Left();
                bytesChainCode = bytesPrivateKeyAndChainCode.Right();

            } while (!IsValidPrivateKey(bytesPrivateKey));

            return new CPrivateKey(bytesPrivateKey, bytesChainCode);
        }

        public CSignature Sign(byte[] hash)
        {
            if (hash == null || hash.Length != 32) throw new ArgumentException("hash length should be 32 bytes", "hash");

            ECDsaSigner signer = new ECDsaSigner(new HMacDsaKCalculator(new Sha3Digest(256)));
            signer.Init(true, Key);

            var rs = signer.GenerateSignature(hash);
            var r = rs[0];
            var s = rs[1];

            CSignature sig = new CSignature(r, s).MakeCanonical();
            byte recoveryParameter = FindRecoveryParameter(sig, hash);
            if (recoveryParameter == 0xff)
                throw new Exception("Sign() Failed");

            sig.V = recoveryParameter;
            return sig;
        }

        private byte FindRecoveryParameter(CSignature sig, byte[] hash)
        {
            byte[] bytesPublicKey = PublicKey.GetPublicKey(false);

            for (byte i = 0; i < 2; i++)
            {
                CPublicKey pubk = sig.ToPublicKey(hash, i);
                if (pubk != null && bytesPublicKey.SequenceEqual(pubk.GetPublicKey(false)))
                {
                    sig.V = i;
                    return i;
                }
            }

            return 0xff;
        }

    }
}
