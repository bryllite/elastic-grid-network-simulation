using Org.BouncyCastle.Crypto.Digests;
using System;
using System.Collections.Generic;
using System.Text;

namespace Bryllite.Core.Hash
{
    public class RIPEMD
    {
        public static byte[] Hash( byte[] bytes )
        {
            return Hash160(bytes);
        }

        public static byte[] Hash( string s )
        {
            return Hash160(Encoding.UTF8.GetBytes(s));
        }

        public static byte[] DoubleHash( byte[] bytes )
        {
            return Hash(Hash(bytes));
        }

        public static byte[] DoubleHash( string s )
        {
            return DoubleHash(Encoding.UTF8.GetBytes(s));
        }

        public static byte[] Hash( byte[] bytes, int bitLength )
        {
            switch( bitLength )
            {
                case 128: return Hash128(bytes);
                case 160: return Hash160(bytes);
                case 256: return Hash256(bytes);
                case 320: return Hash320(bytes);
                default:break;
            }

            throw new ArgumentException("Unsupported digest length");
        }

        public static byte[] Hash128(byte[] bytes)
        {
            var digest = new RipeMD128Digest();
            var output = new byte[digest.GetDigestSize()];

            digest.BlockUpdate(bytes, 0, bytes.Length);
            digest.DoFinal(output, 0);
            return output;
        }

        public static byte[] Hash128(string s)
        {
            return Hash128(Encoding.UTF8.GetBytes(s));
        }

        public static byte[] Hash160(byte[] bytes)
        {
            var digest = new RipeMD160Digest();
            var output = new byte[digest.GetDigestSize()];

            digest.BlockUpdate(bytes, 0, bytes.Length);
            digest.DoFinal(output, 0);
            return output;
        }

        public static byte[] Hash160(string s)
        {
            return Hash160(Encoding.UTF8.GetBytes(s));
        }

        public static byte[] Hash256(byte[] bytes)
        {
            var digest = new RipeMD256Digest();
            var output = new byte[digest.GetDigestSize()];

            digest.BlockUpdate(bytes, 0, bytes.Length);
            digest.DoFinal(output, 0);
            return output;
        }

        public static byte[] Hash256(string s)
        {
            return Hash256(Encoding.UTF8.GetBytes(s));
        }

        public static byte[] Hash320(byte[] bytes)
        {
            var digest = new RipeMD320Digest();
            var output = new byte[digest.GetDigestSize()];

            digest.BlockUpdate(bytes, 0, bytes.Length);
            digest.DoFinal(output, 0);
            return output;
        }

        public static byte[] Hash320(string s)
        {
            return Hash320(Encoding.UTF8.GetBytes(s));
        }
    }
}
