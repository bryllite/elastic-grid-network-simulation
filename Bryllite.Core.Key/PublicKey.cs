using Bryllite.Core.Crypto;
using Bryllite.Core.Hash;
using Bryllite.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bryllite.Core.Key
{
    public class PublicKey
    {
        public static readonly int KEY_BYTES = Secp256k1.PUBLIC_KEY_BYTES;

        private byte[] mBytes;
        public byte[] Bytes
        {
            get
            {
                return mBytes.ToArray();
            }
            private set
            {
                if (value.Length < KEY_BYTES) throw new ArgumentException("Invalid public key bytes");

                Buffer.BlockCopy(value, 0, mBytes, 0, KEY_BYTES);
            }
        }

        public string Hex
        {
            get
            {
                return mBytes.ToHex();
            }
        }

        public Address Address
        {
            get
            {
                return ToETHAddress();
            }
        }

        private PublicKey()
        {
            mBytes = new byte[KEY_BYTES];
        }

        public PublicKey( byte[] bytes ) : this()
        {
            Bytes = bytes;
        }

        public byte[] ToBytes()
        {
            return Bytes;
        }

        public byte[] Encode()
        {
            return Secp256k1.PublicKeySerialize(mBytes, true);
        }

        public static PublicKey Decode( byte[] encode )
        {
            return new PublicKey(Secp256k1.PublicKeyParse(encode));
        }

        private Address ToETHAddress()
        {
            (byte[] left, byte[] right) = mBytes.Reverse().Divide();

            byte[] bytes = right.Merge(left);
            return new Address(SHA3.Hash(bytes).Skip(12).ToArray());
        }

        public static PublicKey FromBytes( byte[] bytes, int offset = 0 )
        {
            return new PublicKey(offset > 0 ? bytes.Skip(offset).ToArray() : bytes);
        }

        public static PublicKey FromPrivateKey( PrivateKey secretKey )
        {
            return FromPrivateKey(secretKey.Bytes);
        }

        public static PublicKey FromPrivateKey( byte[] secretKey )
        {
            return new PublicKey(Secp256k1.PublicKeyCreate(secretKey));
        }

        public override bool Equals(object obj)
        {
            var o = obj as PublicKey;
            return !ReferenceEquals(o, null) && mBytes.SequenceEqual(o.mBytes);
        }

        public override int GetHashCode()
        {
            int hashCode = 0;
            foreach (var c in mBytes)
                hashCode += c;
            return hashCode;
        }

        public static bool operator == (PublicKey left, PublicKey right )
        {
            if (ReferenceEquals(left, null)) return ReferenceEquals(right, null);
            return left.Equals(right);
        }

        public static bool operator != (PublicKey left, PublicKey right )
        {
            return !(left == right);
        }

        public bool Verify( Signature signature, byte[] messageHash )
        {
            return Verify(signature.Bytes, messageHash);
        }

        public bool Verify( byte[] signature, byte[] messageHash )
        {
            return Secp256k1.Verify(signature, messageHash, mBytes);
        }

        public override string ToString()
        {
            return Bytes.ToHex();
        }
    }
}
