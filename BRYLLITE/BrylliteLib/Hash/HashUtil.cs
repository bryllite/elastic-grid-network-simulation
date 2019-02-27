using System;
using System.Collections.Generic;
using System.Text;

namespace BrylliteLib.Hash
{
    public class HashUtil
    {
        public static byte[] Hash256(byte[] value)
        {
            return Sha3.Hash256(value);
        }

        public static byte[] Hash256(string value)
        {
            return Hash256(Encoding.UTF8.GetBytes(value));
        }

        public static byte[] Hash512(byte[] value)
        {
            return Sha3.Hash512(value);
        }

        public static byte[] Hash512(string value)
        {
            return Hash512(Encoding.UTF8.GetBytes(value));
        }
    }
}
