using System;
using System.Collections.Generic;
using System.Text;

namespace BrylliteLib.Net.Elastic
{
    public class ElasticCoordinates
    {
        // 레이아웃 ( 좌표계 )
        private ElasticLayout mLayout;

        // 엘라스틱 목적지 좌표
        private Elastic3D mTo;

        public ElasticLayout Layout
        {
            get
            {
                return mLayout;
            }
        }

        public Elastic3D To
        {
            get
            {
                return mTo;
            }
        }

        public ElasticCoordinates(ElasticLayout layout, Elastic3D to)
        {
            mLayout = new ElasticLayout(layout);
            mTo = new Elastic3D(to);
        }

        private ElasticCoordinates(byte[] bytes, int offset)
        {
            FromBytes(bytes, offset);
        }

        public byte[] ToBytes()
        {
            List<byte> bytes = new List<byte>();
            bytes.AddRange(mLayout.ToBytes());
            bytes.AddRange(mTo.ToBytes());
            return bytes.ToArray();
        }

        private void FromBytes(byte[] bytes, int offset)
        {
            mLayout = ElasticLayout.Parse(bytes, offset);
            mTo = Elastic3D.Parse(bytes, offset + 3);
        }

        public static ElasticCoordinates Parse(byte[] bytes, int offset = 0)
        {
            return new ElasticCoordinates(bytes, offset);
        }

        public static implicit operator byte[] (ElasticCoordinates to)
        {
            return to.ToBytes();
        }

        public static implicit operator ElasticCoordinates(byte[] bytes)
        {
            return new ElasticCoordinates(bytes, 0);
        }

        public override string ToString()
        {
            return $"({mTo.X},{mTo.Y},{mTo.Z}):({mLayout.X},{mLayout.Y},{mLayout.Z})";
        }
    }
}
