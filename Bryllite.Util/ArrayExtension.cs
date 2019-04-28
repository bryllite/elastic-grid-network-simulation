using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace Bryllite.Util
{
    public static class ArrayExtension
    {
        private static RNGCryptoServiceProvider RNG = new RNGCryptoServiceProvider();

        public static T[] Reverse<T>( this T[] array )
        {
            T[] reverse = array.ToArray();
            Array.Reverse(reverse);
            return reverse;
        }

        public static T[] Shuffle<T>( this T[] array )
        {
//            byte[] rnd = new byte[sizeof(long)];
//            RNG.GetBytes(rnd);

            return array.OrderBy(x => RndProvider.Next()).ToArray();
        }

        public static (T[] Left, T[] Right) Divide<T>(this T[] array)
        {
            if (ReferenceEquals(array, null) || array.Length == 0)
                throw new ArgumentException("Indivisible");

            int half = array.Length / 2;
            return (array.Take(half).ToArray(), array.Skip(half).ToArray());
        }

        public static T[] Left<T>( this T[] array )
        {
            return Divide(array).Left;
        }

        public static T[] Right<T>( this T[] array )
        {
            return Divide(array).Right;
        }

        public static T[] Merge<T>( this T[] left, T[] right )
        {
            return left.Concat(right).ToArray();
        }

    }
}
