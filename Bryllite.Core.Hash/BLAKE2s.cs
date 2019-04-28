using Org.BouncyCastle.Crypto.Digests;
using System;
using System.Collections.Generic;
using System.Text;

namespace Bryllite.Core.Hash
{
    public class BLAKE2s
    {
        public static byte[] Hash( byte[] bytes, int bitLength )
        {
            var digest = new Blake2sDigest(bitLength);
            var output = new byte[digest.GetDigestSize()];

            digest.BlockUpdate(bytes, 0, bytes.Length);
            digest.DoFinal(output, 0);

            return output;
        }

        public static byte[] Hash( byte[] bytes )
        {
            return Hash(bytes, 256);
        }

        public static byte[] Hash( string s )
        {
            return Hash(Encoding.UTF8.GetBytes(s), 256);
        }

        public static byte[] DoubleHash( byte[] bytes )
        {
            return Hash(Hash(bytes));
        }

        public static byte[] DoubleHash( string s )
        {
            return DoubleHash(Encoding.UTF8.GetBytes(s));
        }

        public static byte[] Hash64(byte[] bytes)
        {
            return Hash(bytes, 64);
        }

        public static byte[] Hash64(string s)
        {
            return Hash64(Encoding.UTF8.GetBytes(s));
        }

        public static byte[] Hash128(byte[] bytes)
        {
            return Hash(bytes, 128);
        }

        public static byte[] Hash128( string s )
        {
            return Hash128(Encoding.UTF8.GetBytes(s));
        }

        public static byte[] Hash160(byte[] bytes)
        {
            return Hash(bytes, 160);
        }

        public static byte[] Hash160(string s)
        {
            return Hash160(Encoding.UTF8.GetBytes(s));
        }

        public static byte[] Hash224(byte[] bytes)
        {
            return Hash(bytes, 224);
        }

        public static byte[] Hash224(string s)
        {
            return Hash224(Encoding.UTF8.GetBytes(s));
        }

        public static byte[] Hash256(byte[] bytes)
        {
            return Hash(bytes, 256);
        }

        public static byte[] Hash256(string s)
        {
            return Hash256(Encoding.UTF8.GetBytes(s));
        }
    }
}
