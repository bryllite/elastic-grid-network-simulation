using System;
using System.Collections.Generic;
using System.Text;

namespace BrylliteLib.Net.Elastic
{
    public class Elastic3D
    {
        public byte X;
        public byte Y;
        public byte Z;

        public Elastic3D()
        {
        }

        public Elastic3D(byte n) : this(n, n, n)
        {
        }

        public Elastic3D(byte x, byte y, byte z)
        {
            this.X = x;
            this.Y = y;
            this.Z = z;
        }

        public Elastic3D(Elastic3D other)
        {
            this.X = other.X;
            this.Y = other.Y;
            this.Z = other.Z;
        }

        private Elastic3D(byte[] bytes, int offset)
        {
            FromBytes(bytes, offset);
        }

        public bool Solid
        {
            get
            {
                return X > 0 && Y > 0 && Z > 0;
            }
        }

        public byte[] ToBytes()
        {
            return new byte[] { X, Y, Z };
        }

        private void FromBytes(byte[] bytes, int offset)
        {
            this.X = bytes[offset];
            this.Y = bytes[offset + 1];
            this.Z = bytes[offset + 2];
        }

        public static Elastic3D Parse(byte[] bytes, int offset = 0)
        {
            return new Elastic3D(bytes, offset);
        }

        public override string ToString()
        {
            return $"({X},{Y},{Z})";
        }

        public override bool Equals(object obj)
        {
            var other = obj as Elastic3D;
            return other != null && X == other.X && Y == other.Y && Z == other.Z;
        }

        public override int GetHashCode()
        {
            return ToBytes().GetHashCode();
        }

        public static bool operator ==(Elastic3D left, Elastic3D right)
        {
            if (ReferenceEquals(left, null)) return false;
            return left.Equals(right);
        }

        public static bool operator !=(Elastic3D left, Elastic3D right)
        {
            return !(left == right);
        }

        public bool Contains(Elastic3D dest)
        {
            return (X <= 0 || X == dest.X) && (Y <= 0 || Y == dest.Y) && (Z <= 0 || Z == dest.Z);
        }
    }
}
