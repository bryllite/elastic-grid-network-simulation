using BrylliteLib.Crypto;
using BrylliteLib.Hash;
using System;
using System.Collections.Generic;
using System.Text;

namespace BrylliteLib.Net.Elastic
{
    public class ElasticLayout
    {
        private Elastic3D mLayout;

        // 레이아웃의 x,y,z 축 최대값
        public static readonly byte MAX_N = 8;

        public ElasticLayout()
        {
            mLayout = new Elastic3D();
        }

        public ElasticLayout(byte x, byte y, byte z) : this()
        {
            mLayout.X = x;
            mLayout.Y = y;
            mLayout.Z = z;
        }

        public ElasticLayout(byte n) : this()
        {
            mLayout.X = n;
            mLayout.Y = n;
            mLayout.Z = n;
        }

        public ElasticLayout(ElasticLayout layout) : this()
        {
            mLayout.X = layout.X;
            mLayout.Y = layout.Y;
            mLayout.Z = layout.Z;
        }

        private ElasticLayout(byte[] bytes, int offset)
        {
            FromBytes(bytes, offset);
        }

        public byte X => mLayout.X;
        public byte Y => mLayout.Y;
        public byte Z => mLayout.Z;


        public byte[] ToBytes()
        {
            return mLayout.ToBytes();
        }

        private void FromBytes(byte[] bytes, int offset)
        {
            mLayout = Elastic3D.Parse(bytes, offset);
        }

        public static ElasticLayout Parse(byte[] bytes, int offset = 0)
        {
            return new ElasticLayout(bytes, offset);
        }

        public bool Valid()
        {
            if (X <= 0 || X > MAX_N) return false;
            if (Y <= 0 || Y > MAX_N) return false;
            if (Z <= 0 || Z > MAX_N) return false;

            return true;
        }

        public int GridCount
        {
            get
            {
                return X * Y * Z;
            }
        }

        public override bool Equals(object obj)
        {
            var other = obj as ElasticLayout;
            return other != null && mLayout == other.mLayout;
        }

        public override int GetHashCode()
        {
            return mLayout.GetHashCode();
        }

        public static bool operator ==(ElasticLayout left, ElasticLayout right)
        {
            if (ReferenceEquals(left, null) || ReferenceEquals(right, null)) return false;
            return left.Equals(right);
        }

        public static bool operator !=(ElasticLayout left, ElasticLayout right)
        {
            return !(left == right);
        }

        // nPeers 갯수에 따라 레이아웃을 결정한다.
        public static ElasticLayout ComputeLayout(int nPeers)
        {
            for (byte z = 1; z <= MAX_N; z++)
            {
                for (byte y = 1; y <= MAX_N; y++)
                {
                    for (byte x = 1; x <= MAX_N; x++)
                    {
                        if (x * y * z * MAX_N >= nPeers)
                            return new ElasticLayout(x, y, z);
                    }
                }
            }

            return new ElasticLayout(MAX_N);
        }

        public static ElasticLayout ComputeLayoutEx(int nPeers)
        {
            // MAX_N 까지는 분할하지 않고 직접 전송한다.
            if (nPeers <= MAX_N)
                return new ElasticLayout(1, 1, 1);

            // MAX_N 보다 큰 경우, 제곱근(1차원), 세제곱근(2차원), 네제곱근(3차원) 등으로 N을 정하고, 
            // D 차원을 결정한다.
            for (sbyte d = 1; d < 3; d++)
            {
                double y = 1.0 / (d + 1);

                // 반올림 처리
                int layout = (int)Math.Round(Math.Pow(nPeers, y));
                if (layout < MAX_N)
                    return new ElasticLayout((byte)layout, (byte)(d >= 1 ? layout : 1), (byte)(d >= 2 ? layout : 1));
            }

            return new ElasticLayout(MAX_N, MAX_N, MAX_N);
        }

        public Elastic3D AddressToElastic3D(CAddress cAddress)
        {
            if (!Valid()) throw new Exception("Invalid layouts");

            byte[] hash = HashUtil.Hash256(cAddress);

            uint x = 1 + (BitConverter.ToUInt32(hash, 0) % X);
            uint y = 1 + (BitConverter.ToUInt32(hash, 2) % Y);
            uint z = 1 + (BitConverter.ToUInt32(hash, 4) % Z);

            return new Elastic3D((byte)x, (byte)y, (byte)z);
        }

        public static Elastic3D AddressToElastic3D(CAddress cAddress, ElasticLayout layouts)
        {
            return layouts.AddressToElastic3D(cAddress);
        }

        public static implicit operator Elastic3D(ElasticLayout layout)
        {
            return layout.mLayout;
        }

        public static implicit operator ElasticLayout(Elastic3D p)
        {
            return new ElasticLayout(p.X, p.Y, p.Z);
        }

        public override string ToString()
        {
            return mLayout.ToString();
        }
    }
}
