using Org.BouncyCastle.Crypto.Digests;
using System;
using System.Collections.Generic;
using System.Text;

namespace BrylliteLib.Hash
{
    public class Sha3
    {
        private static byte[] Hash(byte[] value, int bitLength)
        {
            // Sha3Digest와 KeccakDigest가 결과가 다르다?
            //            var digest = new Sha3Digest(bitLength);
            var digest = new KeccakDigest(bitLength);
            var output = new byte[digest.GetDigestSize()];

            digest.BlockUpdate(value, 0, value.Length);
            digest.DoFinal(output, 0);
            return output;
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
