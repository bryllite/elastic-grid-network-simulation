using Org.BouncyCastle.Crypto.Digests;
using System;
using System.Text;

namespace Bryllite.Core.Hash
{
    public class SHA3
    {
        private static readonly int DEFAULT_BITLENGTH = 256;

        public static byte[] Hash(byte[] bytes, int bitLength)
        {
            var digest = new KeccakDigest(bitLength);
            var output = new byte[digest.GetDigestSize()];

            digest.BlockUpdate(bytes, 0, bytes.Length);
            digest.DoFinal(output, 0);

            return output;
        }

        public static byte[] Hash( byte[] bytes )
        {
            return Hash(bytes, DEFAULT_BITLENGTH);
        }

        public static byte[] Hash( string s )
        {
            return Hash(Encoding.UTF8.GetBytes(s));
        }

        public static byte[] DoubleHash( byte[] bytes )
        {
            return Hash(Hash(bytes, DEFAULT_BITLENGTH), DEFAULT_BITLENGTH);
        }

        public static byte[] DoubleHash( string s )
        {
            return DoubleHash(Encoding.UTF8.GetBytes(s));
        }

        public static byte[] Hash128( byte[] bytes )
        {
            return Hash(bytes, 128);
        }

        public static byte[] Hash128( string s )
        {
            return Hash(Encoding.UTF8.GetBytes(s), 128);
        }

        public static byte[] Hash224(byte[] bytes)
        {
            return Hash(bytes, 224);
        }

        public static byte[] Hash224(string s)
        {
            return Hash(Encoding.UTF8.GetBytes(s), 224);
        }


        public static byte[] Hash256(byte[] bytes)
        {
            return Hash(bytes, 256);
        }

        public static byte[] Hash256(string s)
        {
            return Hash(Encoding.UTF8.GetBytes(s), 256);
        }


        public static byte[] Hash288(byte[] bytes)
        {
            return Hash(bytes, 288);
        }

        public static byte[] Hash288(string s)
        {
            return Hash(Encoding.UTF8.GetBytes(s), 288);
        }


        public static byte[] Hash384(byte[] bytes)
        {
            return Hash(bytes, 384);
        }

        public static byte[] Hash384(string s)
        {
            return Hash(Encoding.UTF8.GetBytes(s), 384);
        }


        public static byte[] Hash512(byte[] bytes)
        {
            return Hash(bytes, 512);
        }

        public static byte[] Hash512(string s)
        {
            return Hash(Encoding.UTF8.GetBytes(s), 512);
        }

    }
}
