using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Bryllite.Core.Hash;
using Bryllite.Core.Key;
using Newtonsoft.Json.Linq;

namespace Bryllite.Net.Elastic
{
    public class ElasticLayout : Elastic3D
    {
        public static readonly new ElasticLayout Empty = new ElasticLayout();

        // default N value
        public static readonly byte N = 16;

        public ElasticLayout() : base()
        {
        }

        public ElasticLayout( byte x, byte y, byte z) : base( x, y, z )
        {
        }

        public ElasticLayout(ElasticLayout other)
        {
            X = other.X;
            Y = other.Y;
            Z = other.Z;
        }

        private ElasticLayout(byte[] bytes)
        {
            X = bytes[0];
            Y = bytes[1];
            Z = bytes[2];
        }

        private ElasticLayout(byte[] bytes, int offset) : this(offset > 0 ? bytes.Skip(offset).ToArray() : bytes)
        {
        }

        public static new ElasticLayout FromBytes(byte[] bytes, int offset)
        {
            return new ElasticLayout(bytes, offset);
        }

        public static new ElasticLayout FromBytes(byte[] bytes)
        {
            return new ElasticLayout(bytes);
        }

        public override int GetHashCode()
        {
            return BitConverter.ToInt32(ToBytes(), 0).GetHashCode();
        }

        public override bool Equals(object obj)
        {
            var o = obj as ElasticLayout;
            return (!ReferenceEquals(o, null) && X == o.X && Y == o.Y && Z == o.Z);
        }

        public static bool operator ==(ElasticLayout left, ElasticLayout right)
        {
            if (ReferenceEquals(left, null)) return ReferenceEquals(right, null);
            return left.Equals(right);
        }

        public static bool operator !=(ElasticLayout left, ElasticLayout right)
        {
            return !(left == right);
        }

        //public override string ToString()
        //{
        //    string x = X == 0 ? "?" : X.ToString();
        //    string y = Y == 0 ? "?" : Y.ToString();
        //    string z = Z == 0 ? "?" : Z.ToString();

        //    return $"({x},{y},{z})";
        //}

        public static ElasticLayout DefineLayout(int nPeers)
        {
            return DefineLayout(nPeers, N);
        }

        public static ElasticLayout DefineLayout(int nPeers, byte n)
        {
            for (byte z = 1; z <= n; z++)
                for (byte y = 1; y <= n; y++)
                    for (byte x = 1; x <= n; x++)
                        if (nPeers <= n * x * y * z)
                            return new ElasticLayout(x, y, z);

            return new ElasticLayout(n,n,n);
        }

        public Elastic3D DefineCoordinates(Address address)
        {
            if (Mul() <= 0) throw new Exception("Invalid layout");

            byte[] hash = HashProvider.Hash(address.Bytes);

            uint x = 1 + (BitConverter.ToUInt32(hash, 0) % X);
            uint y = 1 + (BitConverter.ToUInt32(hash, 2) % Y);
            uint z = 1 + (BitConverter.ToUInt32(hash, 4) % Z);

            return new Elastic3D((byte)x, (byte)y, (byte)z);
        }

        public static Elastic3D DefineCoordinates(ElasticLayout layout, Address address)
        {
            return layout.DefineCoordinates(address);
        }
    }
}
