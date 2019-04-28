using Org.BouncyCastle.Crypto.Digests;
using System;
using System.Collections.Generic;
using System.Text;

namespace Bryllite.Core.Hash
{
    public class BLAKE2b
    {

        public static byte[] Hash( byte[] bytes, int bitLength )
        {
            var digest = new Blake2bDigest(bitLength);
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
            return Hash(Encoding.UTF8.GetBytes(s));
        }

        public static byte[] DoubleHash( byte[] bytes )
        {
            return Hash(Hash(bytes));
        }

        public static byte[] DoubleHash( string s )
        {
            return DoubleHash(Encoding.UTF8.GetBytes(s));
        }

        public static byte[] Hash128(byte[] value)
        {
            return Hash(value, 128);
        }

        public static byte[] Hash128(string value)
        {
            return Hash128(Encoding.UTF8.GetBytes(value));
        }

        public static byte[] Hash160(byte[] value)
        {
            return Hash(value, 160);
        }

        public static byte[] Hash160(string value)
        {
            return Hash160(Encoding.UTF8.GetBytes(value));
        }

        public static byte[] Hash224(byte[] value)
        {
            return Hash(value, 224);
        }

        public static byte[] Hash224(string value)
        {
            return Hash224(Encoding.UTF8.GetBytes(value));
        }

        public static byte[] Hash256(byte[] value)
        {
            return Hash(value, 256);
        }

        public static byte[] Hash256(string value)
        {
            return Hash256(Encoding.UTF8.GetBytes(value));
        }

        public static byte[] Hash288(byte[] value)
        {
            return Hash(value, 288);
        }

        public static byte[] Hash288(string value)
        {
            return Hash288(Encoding.UTF8.GetBytes(value));
        }

        public static byte[] Hash320(byte[] value)
        {
            return Hash(value, 320);
        }

        public static byte[] Hash320(string value)
        {
            return Hash320(Encoding.UTF8.GetBytes(value));
        }

        public static byte[] Hash384(byte[] value)
        {
            return Hash(value, 384);
        }

        public static byte[] Hash384(string value)
        {
            return Hash384(Encoding.UTF8.GetBytes(value));
        }

        public static byte[] Hash512(byte[] value)
        {
            return Hash(value, 512);
        }
        public static byte[] Hash512(string value)
        {
            return Hash512(Encoding.UTF8.GetBytes(value));
        }

    }
}
