using Org.BouncyCastle.Crypto.Digests;
using System;
using System.Collections.Generic;
using System.Text;

namespace BrylliteLib.Hash
{
    public class Blake2s
    {
        public static byte[] Hash(byte[] value, int bitLength)
        {
            var digest = new Blake2sDigest(bitLength);
            var output = new byte[digest.GetDigestSize()];

            digest.BlockUpdate(value, 0, value.Length);
            digest.DoFinal(output, 0);

            return output;
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

        public static byte[] Hash256(byte[] value)
        {
            return Hash(value, 256);
        }

        public static byte[] Hash256(string value)
        {
            return Hash256(Encoding.UTF8.GetBytes(value));
        }
    }
}
