using System;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace Bryllite.Net.Elastic
{
    public class Elastic3D
    {
        public static readonly Elastic3D Empty = new Elastic3D();

        // x-coordinates
        public byte X;

        // y-coordinates
        public byte Y;

        // z-coordinates
        public byte Z;

        public Elastic3D()
        {
            X = Y = Z = 0;
        }

        public Elastic3D(byte n) : this(n, n, n)
        {
        }

        public Elastic3D(byte x, byte y, byte z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public Elastic3D(Elastic3D other)
        {
            X = other.X;
            Y = other.Y;
            Z = other.Z;
        }

        private Elastic3D(byte[] bytes)
        {
            X = bytes[0];
            Y = bytes[1];
            Z = bytes[2];
        }

        private Elastic3D(byte[] bytes, int offset) : this(offset > 0 ? bytes.Skip(offset).ToArray() : bytes)
        {
        }

        public int Mul()
        {
            return X * Y * Z;
        }

        public virtual byte[] ToBytes()
        {
            return new byte[] { X, Y, Z };
        }

        public static Elastic3D FromBytes(byte[] bytes, int offset)
        {
            return new Elastic3D(bytes, offset);
        }

        public static Elastic3D FromBytes(byte[] bytes)
        {
            return new Elastic3D(bytes);
        }

        public override bool Equals(object obj)
        {
            var o = obj as Elastic3D;
            return (!ReferenceEquals(o, null) && X == o.X && Y == o.Y && Z == o.Z);
        }

        public override int GetHashCode()
        {
            return BitConverter.ToInt32(ToBytes(), 0).GetHashCode();
        }

        public static bool operator ==(Elastic3D left, Elastic3D right)
        {
            if (ReferenceEquals(left, null)) return ReferenceEquals(right, null);
            return left.Equals(right);
        }

        public static bool operator !=(Elastic3D left, Elastic3D right)
        {
            return !(left == right);
        }

        public override string ToString()
        {
            string x = X == 0 ? "?" : X.ToString();
            string y = Y == 0 ? "?" : Y.ToString();
            string z = Z == 0 ? "?" : Z.ToString();

            return $"({x},{y},{z})";
        }

        public bool Contains(Elastic3D pos)
        {
            return (X == 0 || X == pos.X) && (Y == 0 || Y == pos.Y) && (Z == 0 || Z == pos.Z);
        }
    }
}
