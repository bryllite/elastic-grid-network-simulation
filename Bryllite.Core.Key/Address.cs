using Bryllite.Core.Hash;
using Bryllite.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bryllite.Core.Key
{
    public class Address
    {
        public static readonly int ADDRESS_BYTES = 20;
        public static readonly Address Empty = new Address();

        private byte[] mBytes;
        public byte[] Bytes
        {
            get
            {
                return mBytes.ToArray();
            }
            private set
            {
                if (value.Length < ADDRESS_BYTES) throw new ArgumentException("Invalid address bytes");
                Buffer.BlockCopy(value, 0, mBytes, 0, ADDRESS_BYTES);
            }
        }

        // hex string only without hex prefix
        public string Hex => mBytes.ToHex();

        // EIP55: checksum address with hex prefix
        public string HexAddress => ToEIP55(mBytes.ToHex(true));

        private Address()
        {
            mBytes = new byte[ADDRESS_BYTES];
        }

        public Address(byte[] bytes) : this()
        {
            Bytes = bytes;
        }

        public Address(Address other) : this( other.Bytes )
        {
        }

        public Address( byte[] bytes, int offset ) : this( bytes.Skip(offset).ToArray())
        {
        }

        public Address(string addr) : this(addr.HexToBytes())
        {
        }

        public static Address FromBytes(byte[] bytes, int offset)
        {
            return new Address(bytes, offset);
        }

        public static Address FromBytes(byte[] bytes)
        {
            return new Address(bytes);
        }


        public static explicit operator string(Address addr)
        {
            return addr.Hex;
        }

        public static explicit operator Address(string s)
        {
            return new Address(s);
        }

        public static explicit operator byte[] (Address addr)
        {
            return addr.Bytes;
        }

        public static explicit operator Address(byte[] bytes)
        {
            return new Address(bytes);
        }

        public override bool Equals(object obj)
        {
            var o = obj as Address;
            return (!ReferenceEquals(o, null) && mBytes.SequenceEqual(o.mBytes));
        }

        public override int GetHashCode()
        {
            int hashCode = 0;
            foreach (var c in mBytes)
                hashCode += c;
            return hashCode;
        }

        public static bool operator==(Address left, Address right)
        {
            if (ReferenceEquals(left, null)) return ReferenceEquals(right, null);
            return left.Equals(right);
        }

        public static bool operator!=(Address left, Address right)
        {
            return !(left == right);
        }

        public override string ToString()
        {
            return HexExtension.HexPrefix + Hex;
        }

        public static string ToEIP55( string address )
        {
            bool prefix = address.HasHexPrefix();
            string addr = address.ToLower().RemoveHexPrefix();
            string hash = SHA3.Hash256(addr).ToHex();

            string eip55 = "";
            for ( int i = 0; i < addr.Length; i++ )
            {
                string c = hash[i].ToString();
                eip55 += Convert.ToByte(hash[i].ToString(), 16) >= 8 ? addr[i].ToString().ToUpper() : addr[i].ToString();
            }

            return ( prefix ? HexExtension.HexPrefix : "" ) + eip55;
        }
    }
}
