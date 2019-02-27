using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace BrylliteLib.Utils
{
    public class RndGenerator
    {
        private static Random mRND = new Random();
        private static RNGCryptoServiceProvider mRNGScryptoServiceProvider = new RNGCryptoServiceProvider();

        private static readonly string NUMERICS_AND_ALPHABETS = "0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";

        public static byte[] GetBytes(int bytes)
        {
            if (bytes <= 0)
                throw new ArgumentException("bytes must be > 0");

            byte[] byteArray = new byte[bytes];
            mRNGScryptoServiceProvider.GetNonZeroBytes(byteArray);

            return byteArray;
        }

        public static byte[] GetNonZeroBytes(int bytes)
        {
            if (bytes <= 0)
                throw new ArgumentException("bytes must be > 0");

            byte[] byteArray = new byte[bytes];
            mRNGScryptoServiceProvider.GetNonZeroBytes(byteArray);

            return byteArray;
        }

        public static byte GetByte()
        {
            return GetBytes(1)[0];
        }

        public static short GetShort()
        {
            return BitConverter.ToInt16(GetBytes(2), 0);
        }

        public static ushort GetUShort()
        {
            return BitConverter.ToUInt16(GetBytes(2), 0);
        }

        public static int GetInt()
        {
            return BitConverter.ToInt32(GetBytes(4), 0);
        }

        public static uint GetUInt()
        {
            return BitConverter.ToUInt32(GetBytes(4), 0);
        }

        public static long GetLong()
        {
            return BitConverter.ToInt64(GetBytes(8), 0);
        }

        public static ulong GetULong()
        {
            return BitConverter.ToUInt64(GetBytes(8), 0);
        }

        public static string GetString(int length)
        {
            var chars = Enumerable.Range(0, length).Select(x => NUMERICS_AND_ALPHABETS[mRND.Next(0, NUMERICS_AND_ALPHABETS.Length)]);
            return new string(chars.ToArray());
        }

        public static string[] GetStrings(int count, int length)
        {
            List<string> strings = new List<string>();

            for (int i = 0; i < count; i++)
                strings.Add(GetString(length));

            return strings.ToArray();
        }

        public static int Next()
        {
            return mRND.Next();
        }

        public static int Next(int maxValue)
        {
            return mRND.Next(maxValue);
        }

        public static int Next(int minValue, int maxValue)
        {
            return mRND.Next(minValue, maxValue);
        }
    }
}
