using BrylliteLib.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BrylliteLib.Crypto
{
    public class CAddress
    {
        public static readonly int ADDRESS_BYTES = 20;
        public static readonly byte[] Null = new byte[ADDRESS_BYTES];   // filled with all 0 byte?

        // 계좌 주소 바이트 배열
        private byte[] _bytes;

        public byte[] Bytes
        {
            get
            {
                return _bytes.ToArray();
            }
        }

        public string HexAddress
        {
            get
            {
                return UtilsExtensions.HexPrefix + _bytes.ToHexString();
            }
        }

        public CAddress()
        {
            _bytes = new byte[ADDRESS_BYTES];
        }

        public CAddress(byte[] bytes) : this()
        {
            if (bytes.Length != ADDRESS_BYTES)
                throw new ArgumentException("Invalid address bytes length");

            Buffer.BlockCopy(bytes, 0, _bytes, 0, ADDRESS_BYTES);
        }

        public CAddress(string address) : this()
        {
            byte[] bytes = address.RemoveHexPrefix().HexToByteArray();
            if (bytes.Length != ADDRESS_BYTES)
                throw new ArgumentException("Invalid address string length");

            Buffer.BlockCopy(bytes, 0, _bytes, 0, ADDRESS_BYTES);
        }

        public static implicit operator byte[] (CAddress addr)
        {
            return addr._bytes;
        }

        public static implicit operator CAddress(string str)
        {
            return new CAddress(str);
        }

        public static implicit operator CAddress(byte[] bytes)
        {
            return new CAddress(bytes);
        }

        public override bool Equals(object obj)
        {
            var other = obj as CAddress;
            if (ReferenceEquals(other, null)) return false;

            return _bytes.SequenceEqual(other._bytes);
        }

        public static bool operator ==(CAddress left, CAddress right)
        {
            if (ReferenceEquals(left, null) || ReferenceEquals(right, null)) return false;
            return left._bytes.SequenceEqual(right._bytes);
        }

        public static bool operator !=(CAddress left, CAddress right)
        {
            return !(left == right);
        }

        public override int GetHashCode()
        {
            return (_bytes == null) ? 0 : _bytes.GetHashCode();
        }

        public static bool IsValidAddress(string address)
        {
            byte[] bytes = address.RemoveHexPrefix().HexToByteArray();
            return bytes.Length == ADDRESS_BYTES;
        }
    }
}
