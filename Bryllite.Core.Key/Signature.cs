using Bryllite.Core.Crypto;
using Bryllite.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bryllite.Core.Key
{
    public class Signature
    {
        public static readonly int SIGNATURE_BYTES = Secp256k1.SIGNATURE_BYTES;

        public static readonly Signature Empty = new Signature();

        private byte[] mBytes;
        public byte[] Bytes
        {
            get
            {
                return mBytes.ToArray();
            }
            private set
            {
                if (value.Length < SIGNATURE_BYTES) throw new ArgumentException("Invalid signature bytes");
                Buffer.BlockCopy(value, 0, mBytes, 0, SIGNATURE_BYTES);
            }
        }

        public string Hex
        {
            get
            {
                return mBytes.ToHex();
            }
        }

        private Signature()
        {
            mBytes = new byte[SIGNATURE_BYTES];
        }

        public Signature(byte[] bytes) : this()
        {
            Bytes = bytes;
        }

        public Signature(Signature other) : this(other.Bytes)
        {
        }

        public Signature( byte[] bytes, int offset ) : this( offset > 0 ? bytes.Skip(offset).ToArray() : bytes )
        {
        }

        public PublicKey ToPublicKey( byte[] messageHash )
        {
            return new PublicKey( Secp256k1.Recover(mBytes, messageHash) );
        }

        public byte[] ToBytes()
        {
            return Bytes;
        }

        public static Signature FromBytes( byte[] bytes, int offset )
        {
            return new Signature(bytes, offset);
        }

        public static Signature FromBytes( byte[] bytes )
        {
            return new Signature(bytes);
        }

        public override bool Equals(object obj)
        {
            var o = obj as Signature;
            return !ReferenceEquals(o, null) && mBytes.SequenceEqual(o.mBytes);
        }

        public override int GetHashCode()
        {
            int hashCode = 0;
            foreach (var c in mBytes)
                hashCode += c;
            return hashCode;
        }

        public static bool operator == (Signature left, Signature right )
        {
            if (ReferenceEquals(left, null)) return ReferenceEquals(right, null);
            return left.Equals(right);
        }

        public static bool operator != (Signature left, Signature right )
        {
            return !(left == right);
        }

        public static implicit operator byte[]( Signature s )
        {
            return s.Bytes;
        }

        public static explicit operator Signature(byte[] bytes)
        {
            return new Signature(bytes);
        }

        public override string ToString()
        {
            return Bytes.ToHex();
        }
    }
}
