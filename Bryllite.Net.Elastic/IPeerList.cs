using System;
using System.Collections.Generic;
using System.Text;
using Bryllite.Core.Key;

namespace Bryllite.Net.Elastic
{
    public interface IPeerList
    {
        int Count { get; }

        string[] Peers { get; }

        void LoadDB();
        void LoadDB(string file);

        void SaveDB();
        void SaveDB(string file);

        void Append(ElasticAddress address);
        void Append(ElasticAddress[] addresses);
        void Append(string address);
        void Append(string[] addresses);

        void Remove(Address address);
        void Remove(Address[] addresses);
        void Remove(ElasticAddress address);
        void Remove(ElasticAddress[] addresses);
        void Remove(string address);
        void Remove(string[] addresses);

        void Clear();

        void Update(string[] catalogs);

        ElasticAddress Find(string address);
        ElasticAddress Find(Address address);

        bool Exists(string address);
        bool Exists(Address address);
        bool Exists(ElasticAddress address);
        bool Exists(string host, int port);

        T[] ToArray<T>();
        T[] ToArray<T>(ElasticAddress[] peers);
        T[] ToArray<T>(Elastic3D coordinates, ElasticLayout layout);

        //byte[] ComputeMerkleHash(string[] catalogs);
    }
}
