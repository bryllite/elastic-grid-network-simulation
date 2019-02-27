using Org.BouncyCastle.Crypto.Digests;
using System;
using System.Collections.Generic;
using System.Text;

namespace BrylliteLib.Hash
{
    public class RipeMD
    {
        public static byte[] Hash128(byte[] value)
        {
            var digest = new RipeMD128Digest();
            var output = new byte[digest.GetDigestSize()];

            digest.BlockUpdate(value, 0, value.Length);
            digest.DoFinal(output, 0);
            return output;
        }

        public static byte[] Hash128(string value)
        {
            return Hash128(Encoding.UTF8.GetBytes(value));
        }

        public static byte[] Hash160(byte[] value)
        {
            var digest = new RipeMD160Digest();
            var output = new byte[digest.GetDigestSize()];

            digest.BlockUpdate(value, 0, value.Length);
            digest.DoFinal(output, 0);
            return output;
        }

        public static byte[] Hash160(string value)
        {
            return Hash160(Encoding.UTF8.GetBytes(value));
        }

        public static byte[] Hash256(byte[] value)
        {
            var digest = new RipeMD256Digest();
            var output = new byte[digest.GetDigestSize()];

            digest.BlockUpdate(value, 0, value.Length);
            digest.DoFinal(output, 0);
            return output;
        }

        public static byte[] Hash256(string value)
        {
            return Hash256(Encoding.UTF8.GetBytes(value));
        }

        public static byte[] Hash320(byte[] value)
        {
            var digest = new RipeMD320Digest();
            var output = new byte[digest.GetDigestSize()];

            digest.BlockUpdate(value, 0, value.Length);
            digest.DoFinal(output, 0);
            return output;
        }

        public static byte[] Hash320(string value)
        {
            return Hash320(Encoding.UTF8.GetBytes(value));
        }
    }
}
