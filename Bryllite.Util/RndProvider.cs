using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace Bryllite.Util
{
    public class RndProvider
    {
        public const string ALPHA_NUMERICS = "0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";

        private static RNGCryptoServiceProvider mRNGCryptoServiceProvider = new RNGCryptoServiceProvider();

        public static byte[] GetBytes( int length )
        {
            if (length <= 0) throw new ArgumentException("length must be over 0");

            byte[] bytes = new byte[length];
            mRNGCryptoServiceProvider.GetBytes(bytes);
            return bytes;
        }

        public static byte[] GetNonZeroBytes( int length )
        {
            if (length <= 0) throw new ArgumentException("length must be over 0");

            byte[] bytes = new byte[length];
            mRNGCryptoServiceProvider.GetNonZeroBytes(bytes);
            return bytes;
        }

        public static byte GetByte()
        {
            return GetBytes(1)[0];
        }

        public static sbyte GetSByte()
        {
            return Convert.ToSByte(GetByte());
        }

        public static short GetShort()
        {
            return BitConverter.ToInt16(GetBytes(sizeof(short)), 0);
        }

        public static ushort GetUShort()
        {
            return BitConverter.ToUInt16(GetBytes(sizeof(ushort)), 0);
        }

        public static int GetInt()
        {
            return BitConverter.ToInt32(GetBytes(sizeof(int)), 0);
        }

        public static uint GetUInt()
        {
            return BitConverter.ToUInt32(GetBytes(sizeof(uint)), 0);
        }

        public static long GetLong()
        {
            return BitConverter.ToInt64(GetBytes(sizeof(long)), 0);
        }

        public static ulong GetUlong()
        {
            return BitConverter.ToUInt64(GetBytes(sizeof(ulong)), 0);
        }

        public static string GetString(int length)
        {
            var chars = Enumerable.Range(0, length).Select(c => ALPHA_NUMERICS[Next(ALPHA_NUMERICS.Length)]);
            return new string(chars.ToArray());
        }

        public static int Next()
        {
            return Math.Abs(GetInt());
        }

        public static int Next(int maxValue)
        {
            return Next() % maxValue;
        }

        public static int Next(int minValue, int maxValue)
        {
            return minValue + (Next() % (maxValue - minValue));
        }

        public static T[] Shuffle<T>(T[] array)
        {
            T[] shuffled = array.ToArray();
            int n = shuffled.Length;
            while (--n > 0)
            {
                int k = Next(n + 1);
                T temp = shuffled[k];
                shuffled[k] = shuffled[n];
                shuffled[n] = temp;
            }

            return shuffled;
        }

    }
}
