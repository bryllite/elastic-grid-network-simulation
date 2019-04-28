using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bryllite.Util
{
    public static class HexExtension
    {
        public static readonly string HexPrefix = "0x";
        public static readonly string HexChars = "0123456789abcdefABCDEF";

        public static bool HasHexPrefix(this string hex)
        {
            return !string.IsNullOrEmpty(hex) ? hex.ToLower().StartsWith(HexPrefix) : false;
        }

        public static string RemoveHexPrefix( this string hex )
        {
            return !HasHexPrefix(hex) ? hex : hex.Substring(HexPrefix.Length, hex.Length - HexPrefix.Length);
        }

        public static string ToHex( this byte[] bytes )
        {
            return ToLowerHex(bytes);
        }

        public static string ToHex( this byte[] bytes, bool prefix )
        {
            return ToLowerHex(bytes, prefix);
        }

        public static string ToLowerHex( this byte[] bytes )
        {
            return ToLowerHex(bytes, false);
        }

        public static string ToLowerHex( this byte[] bytes, bool prefix )
        {
            return (prefix ? HexPrefix : "") + string.Concat(bytes.Select(c => c.ToString("x2")).ToArray());
        }

        public static string ToUpperHex( this byte[] bytes )
        {
            return ToUpperHex(bytes);
        }

        public static string ToUpperHex( this byte[] bytes, bool prefix )
        {
            return (prefix ? HexPrefix : "") + string.Concat(bytes.Select(c => c.ToString("X2")).ToArray());
        }

        public static bool IsHex( this string hex )
        {
            if (string.IsNullOrEmpty(hex) || hex.Length % 2 != 0) return false;
            return RemoveHexPrefix(hex).All(HexChars.Contains);
        }

        public static byte[] HexToBytes( this string hex )
        {
            if (!IsHex(hex)) throw new ArgumentException("not hex string");

            List<byte> bytes = new List<byte>();

            hex = RemoveHexPrefix(hex);
            for (int i = 0; i < hex.Length; i += 2)
                bytes.Add(Convert.ToByte(hex.Substring(i, 2), 16));

            return bytes.ToArray();
        }

        public static string Ellipsis(this byte[] bytes)
        {
            return Ellipsis(bytes, 3, 3);
        }

        public static string Ellipsis(this byte[] bytes, int head)
        {
            return Ellipsis(bytes, head, 0);
        }


        public static string Ellipsis(this byte[] bytes, int head, int tail)
        {
            return Ellipsis(bytes.ToHex(), head * 2, tail * 2);
        }

        public static string Ellipsis(this string hex)
        {
            return Ellipsis(hex, 6, 6);
        }

        public static string Ellipsis(this string hex, int head)
        {
            return Ellipsis(hex, head, 0);
        }

        public static string Ellipsis(this string hex, int head, int tail)
        {
            if (hex.Length < head + tail) throw new ArgumentException("exceeds ellipsis length");

            StringBuilder sb = new StringBuilder();
            sb.Append(hex.Substring(0, head));
            sb.Append("…");

            if (tail > 0)
                sb.Append(hex.Substring(hex.Length - tail, tail));

            return sb.ToString();
        }
    }
}
