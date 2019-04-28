using System;

namespace Bryllite.Core.Hash
{
    public class HashProvider
    {
        public static byte[] Hash(byte[] bytes)
        {
            return SHA3.Hash(bytes);
        }

        public static byte[] Hash(string s)
        {
            return SHA3.Hash(s);
        }

        public static byte[] DoubleHash(byte[] bytes)
        {
            return SHA3.DoubleHash(bytes);
        }

        public static byte[] DoubleHash(string s)
        {
            return SHA3.DoubleHash(s);
        }

        public static byte[] Hash160(byte[] bytes)
        {
            return RIPEMD.Hash160(bytes);
        }

        public static byte[] Hash160(string s)
        {
            return RIPEMD.Hash160(s);
        }

        public static byte[] Hash256(byte[] bytes)
        {
            return SHA3.Hash256(bytes);
        }

        public static byte[] Hash256(string s)
        {
            return SHA3.Hash256(s);
        }

        public static byte[] Hash512(byte[] bytes)
        {
            return SHA3.Hash512(bytes);
        }

        public static byte[] Hash512(string s)
        {
            return SHA3.Hash512(s);
        }


        public static byte[] Hash( byte[] bytes, string algorithm, int bitLength )
        {
            switch( algorithm )
            {
                case "RIPEMD": return RIPEMD.Hash(bytes, bitLength);
                case "SHA256": return SHA256.Hash(bytes, bitLength);
                case "SHA3": return SHA3.Hash(bytes, bitLength);
                case "BLAKE2s": return BLAKE2s.Hash(bytes, bitLength);
                case "BLAKE2b": return BLAKE2b.Hash(bytes, bitLength);
                default: break;
            }

            throw new ArgumentException("Unknown hash algorithm");
        }
    }
}
