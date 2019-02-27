using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace BrylliteLib.Utils
{
    public static class UtilsExtensions
    {
        public static readonly string HexPrefix = "0x";
        public static readonly byte[] ByteEmpty = new byte[0];

        public static bool HasHexPrefix(this string hex)
        {
            return (!string.IsNullOrEmpty(hex)) ? hex.ToLower().StartsWith(HexPrefix) : false;
        }

        public static string RemoveHexPrefix(this string hex)
        {
            return hex.HasHexPrefix() ? hex.Substring(HexPrefix.Length, hex.Length - HexPrefix.Length) : hex;
        }

        public static string ToHexString(this byte[] bytes, bool upper = false)
        {
            return string.Concat(bytes.Select(b => b.ToString(upper ? "X2" : "x2")).ToArray());
        }

        public static bool IsHexString(this string hex)
        {
            if (string.IsNullOrEmpty(hex) || hex.Length % 2 != 0) return false;

            foreach (var c in hex)
            {
                if (!((c >= '0' && c <= '9') || (c >= 'a' && c <= 'f') || (c >= 'A' && c <= 'F')))
                    return false;
            }

            return true;
        }

        public static string SimpleHexString(this byte[] bytes, int showBytes = 4)
        {
            if (showBytes >= bytes.Length)
                return bytes.ToHexString();

            return bytes.Take(showBytes).ToArray().ToHexString() + "...";
        }

        public static byte[] HexToByteArray(this string hex)
        {
            string str = hex.RemoveHexPrefix();
            if (!str.IsHexString()) return ByteEmpty;

            List<byte> listBytes = new List<byte>();
            for (int i = 0; i < str.Length; i += 2)
                listBytes.Add(Convert.ToByte(str.Substring(i, 2), 16));

            return listBytes.ToArray();
        }

        public static byte[] Left(this byte[] bytes)
        {
            if (bytes == null || bytes.Length <= 0 || bytes.Length % 2 != 0)
                throw new ArgumentException("Invalid byte array", "bytes");

            return bytes.Take(bytes.Length / 2).ToArray();
        }

        public static byte[] Right(this byte[] bytes)
        {
            if (bytes == null || bytes.Length <= 0 || bytes.Length % 2 != 0)
                throw new ArgumentException("Invalid byte array", "bytes");

            return bytes.Skip(bytes.Length / 2).ToArray();
        }

        // 배열을 섞는다.
        public static T[] Shuffle<T>(this T[] arr)
        {
            return arr.OrderBy(x => RndGenerator.GetUInt()).ToArray();
        }

        public static T[] Add<T>(this T[] left, T[] right)
        {
            return left.Concat(right).ToArray();
        }

        public static bool BitAt(this byte val, int index)
        {
            Debug.Assert(index >= 0 && index < 8);
            return Convert.ToBoolean(val & (0x01 << index));
        }

        public static bool BitAt(this byte[] bytes, int index)
        {
            Debug.Assert(index >= 0 && bytes.Length * 8 > index);
            return bytes.ToBitList()[index];
        }

        public static List<bool> ToBitList(this byte[] bytes)
        {
            List<bool> listBits = new List<bool>();

            for (int i = 0; i < bytes.Length; i++)
                for (int pos = 0; pos < 8; pos++)
                    listBits.Add(BitAt(bytes[i], pos));

            return listBits;
        }
    }
}
