using Org.BouncyCastle.Crypto.Digests;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace Bryllite.Core.Hash
{
    public class SHA256
    {
        public static byte[] Hash( byte[] bytes )
        {
            return Hash256(bytes);
        }

        public static byte[] DoubleHash( byte[] bytes )
        {
            return Hash256(Hash256(bytes));
        }

        public static byte[] Hash( string s )
        {
            return Hash256(Encoding.UTF8.GetBytes(s));
        }

        public static byte[] DoubleHash( string s )
        {
            return DoubleHash(Encoding.UTF8.GetBytes(s));
        }

        public static byte[] Hash( byte[] bytes, int bitLength )
        {
            switch( bitLength )
            {
                case 224: return Hash224(bytes);
                case 256: return Hash256(bytes);
                case 384: return Hash384(bytes);
                case 512: return Hash512(bytes);
                default:break;
            }

            throw new ArgumentException("Unsupported digest length");
        }

        public static byte[] Hash224( byte[] bytes )
        {
            var digest = new Sha224Digest();
            var output = new byte[digest.GetDigestSize()];

            digest.BlockUpdate(bytes, 0, bytes.Length);
            digest.DoFinal(output, 0);

            return output;
        }

        public static byte[] Hash224( string s )
        {
            return Hash224(Encoding.UTF8.GetBytes(s));
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

        // HMAC-SHA256

        public static byte[] HMAC256( string key, byte[] message )
        {
            return HMAC256(Encoding.UTF8.GetBytes(key), message);
        }

        public static byte[] HMAC256( string key, string message )
        {
            return HMAC256(Encoding.UTF8.GetBytes(key), Encoding.UTF8.GetBytes(message));
        }

        public static byte[] HMAC256( byte[] key, byte[] message )
        {
            return new HMACSHA256(key).ComputeHash(message);
        }

        // HMAC-SHA512

        public static byte[] HMAC512( string key, byte[] message )
        {
            return HMAC512(Encoding.UTF8.GetBytes(key), message);
        }

        public static byte[] HMAC512( string key, string message )
        {
            return HMAC512(Encoding.UTF8.GetBytes(key), Encoding.UTF8.GetBytes(message));
        }

        public static byte[] HMAC512(byte[] key, byte[] message )
        {
            return new HMACSHA512(key).ComputeHash(message);
        }
    }

}
