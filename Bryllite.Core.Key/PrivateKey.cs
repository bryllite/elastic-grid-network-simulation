using Bryllite.Util;
using Bryllite.Core.Crypto;
using Bryllite.Core.Hash;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;

namespace Bryllite.Core.Key
{
    public class PrivateKey
    {
        public static readonly int KEY_BYTES = Secp256k1.PRIVATE_KEY_BYTES;
        public static readonly int CHAINCODE_BYTES = 32;

        private byte[] mBytes;
        public byte[] Bytes
        {
            get
            {
                return mBytes.ToArray();
            }
            private set
            {
                if (value.Length < KEY_BYTES) throw new ArgumentException("Invalid private key bytes");
                mBytes = value.ToArray();
            }
        }

        public string Hex
        {
            get
            {
                return mBytes.ToHex();
            }
        }

        private byte[] mChainCode = null ;
        public byte[] ChainCode
        {
            get
            {
                return mChainCode == null ? HashProvider.Hash(Bytes) : mChainCode.ToArray();
            }
            private set
            {
                if (value.Length < CHAINCODE_BYTES) throw new ArgumentException("Invalid chain code bytes");
                if (mChainCode == null) mChainCode = new byte[CHAINCODE_BYTES];
                Buffer.BlockCopy(value, 0, mChainCode, 0, CHAINCODE_BYTES);
            }
        }

        public string HexChainCode
        {
            get
            {
                return ChainCode.ToHex();
            }
        }

        private PrivateKey()
        {
            mBytes = new byte[KEY_BYTES];
        }

        public PrivateKey( string hex ) : this( hex.HexToBytes() )
        {
        }

        public PrivateKey( byte[] bytes ) : this()
        {
            Bytes = bytes;
        }

        public PrivateKey( byte[] secretKey, byte[] chainCode ) : this( secretKey )
        {
            ChainCode = chainCode;
        }

        public byte[] ToBytes()
        {
            return Bytes.Merge(ChainCode);
        }

        public static PrivateKey FromBytes(byte[] bytes, int offset = 0)
        {
            byte[] b = bytes.Skip(offset).ToArray();
            PrivateKey privKey = new PrivateKey(b);

            if (b.Length >= KEY_BYTES + CHAINCODE_BYTES)
                privKey.ChainCode = b.Skip(KEY_BYTES).ToArray();

            return privKey;
        }

        public static PrivateKey CreateKey()
        {
            byte[] bytes = RndProvider.GetNonZeroBytes(KEY_BYTES);
            return Secp256k1.SecretKeyVerify(bytes) ? new PrivateKey(bytes) : CreateKey();
        }


        public PublicKey PublicKey
        {
            get
            {
                return PublicKey.FromPrivateKey(Bytes);
            }
        }

        public Address Address
        {
            get
            {
                return PublicKey.Address;
            }
        }

        public override bool Equals(object obj)
        {
            var o = obj as PrivateKey;
            return !ReferenceEquals(o, null) && mBytes.SequenceEqual(o.mBytes) && ChainCode.SequenceEqual(o.ChainCode );
        }

        public override int GetHashCode()
        {
            int hashCode = 0;
            foreach (var c in mBytes)
                hashCode += c;
            foreach (var c in ChainCode)
                hashCode += c;
            return hashCode;
        }

        public static bool operator==(PrivateKey left, PrivateKey right )
        {
            if (ReferenceEquals(left, null)) return ReferenceEquals(right, null);
            return left.Equals(right);
        }

        public static bool operator!=(PrivateKey left, PrivateKey right )
        {
            return !(left == right);
        }

        public Signature Sign( byte[] messageHash )
        {
            return new Signature(Secp256k1.SignRecoverable(messageHash, mBytes) );
        }

        private PrivateKey ChildKeyDerive(byte[] keyPath)
        {
            byte[] childKey;
            byte[] childChainCode;
            byte nonce = 0;

            do
            {
                List<byte> bytes = new List<byte>();

                bytes.AddRange(Bytes);
                bytes.AddRange(keyPath);
                bytes.AddRange(ChainCode);
                bytes.Add(nonce++);

                byte[] keyAndChainCode = HashProvider.Hash512(bytes.ToArray());
                childKey = keyAndChainCode.Left();
                childChainCode = keyAndChainCode.Right();
            } while (!Secp256k1.SecretKeyVerify(childKey));

            return new PrivateKey(childKey, childChainCode);
        }

        public PrivateKey CKD( byte[] keyPath )
        {
            return ChildKeyDerive(keyPath);
        }

        public PrivateKey CKD( string keyPath )
        {
            return CKD(keyPath.Split('/'));
        }

        public PrivateKey CKD( string[] keyPath )
        {
            if (keyPath == null || keyPath.Length == 0) throw new ArgumentNullException("keyPath required");

            PrivateKey key = this;
            for (int i = 0; i < keyPath.Length; i++)
                key = key.CKD(Encoding.UTF8.GetBytes(keyPath[i]));

            return key;
        }

        public override string ToString()
        {
            return Bytes.ToHex();
        }
    }
}
