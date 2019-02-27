using Org.BouncyCastle.Crypto.Digests;
using System;
using System.Collections.Generic;
using System.Text;

namespace BrylliteLib.Hash
{
    public class Sha2
    {
        public static byte[] Hash224(byte[] value)
        {
            var digest = new Sha224Digest();
            var output = new byte[digest.GetDigestSize()];

            digest.BlockUpdate(value, 0, value.Length);
            digest.DoFinal(output, 0);

            return output;
        }

        public static byte[] Hash224(string value)
        {
            return Hash224(Encoding.UTF8.GetBytes(value));
        }

        public static byte[] Hash256(byte[] value)
        {
            var digest = new Sha256Digest();
            var output = new byte[digest.GetDigestSize()];

            digest.BlockUpdate(value, 0, value.Length);
            digest.DoFinal(output, 0);

            return output;
        }

        public static byte[] Hash256(string value)
        {
            return Hash256(Encoding.UTF8.GetBytes(value));
        }

        public static byte[] Hash384(byte[] value)
        {
            var digest = new Sha384Digest();
            var output = new byte[digest.GetDigestSize()];

            digest.BlockUpdate(value, 0, value.Length);
            digest.DoFinal(output, 0);

            return output;
        }

        public static byte[] Hash384(string value)
        {
            return Hash384(Encoding.UTF8.GetBytes(value));
        }


        public static byte[] Hash512(byte[] value)
        {
            var digest = new Sha512Digest();
            var output = new byte[digest.GetDigestSize()];

            digest.BlockUpdate(value, 0, value.Length);
            digest.DoFinal(output, 0);

            return output;
        }

        public static byte[] Hash512(string value)
        {
            return Hash512(Encoding.UTF8.GetBytes(value));
        }

    }
}
