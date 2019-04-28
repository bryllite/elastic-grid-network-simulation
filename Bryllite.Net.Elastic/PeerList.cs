using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Bryllite.Core.Hash;
using Bryllite.Core.Key;
using Bryllite.Util;
using Bryllite.Util.Log;
using Newtonsoft.Json.Linq;

namespace Bryllite.Net.Elastic
{
    public class PeerList : IPeerList
    {
        public static readonly string DBName = "PeerList.json";

        // logger
        private ILoggable Logger;

        // Node List
        private Dictionary<Address, ElasticAddress> _peers;

        public int Count => _peers.Count;

        public string[] Peers => ToArray<string>();

        public PeerList()
        {
            _peers = new Dictionary<Address, ElasticAddress>();
        }

        public PeerList(ILoggable logger) : this()
        {
            Logger = logger;
        }


        public void LoadDB()
        {
            LoadDB(DBName);
        }

        public void LoadDB(string file)
        {
            // load catalogs file
            if (File.Exists(file))
            {
                string json = File.ReadAllText(file);
                JObject jObject = JObject.Parse(json);
                JArray peers = jObject.GetValue("nodes") as JArray;

                foreach (var peer in peers.Values<string>())
                    Append(new ElasticAddress(peer));
            }
        }

        public void SaveDB()
        {
            SaveDB(DBName);
        }

        public void SaveDB(string file)
        {
            string[] peers = ToArray<string>();

            JObject jObject = new JObject();
            jObject["nodes"] = new JArray(peers);
            File.WriteAllText(file, jObject.ToString());
        }

        public void Append(ElasticAddress peer)
        {
            lock (_peers)
            {
                _peers[peer.Address] = peer;
            }
        }

        public void Append(ElasticAddress[] peers)
        {
            foreach (var peer in peers)
                Append(peer);
        }


        public void Append(string peer)
        {
            Append(new ElasticAddress(peer));
        }

        public void Append(string[] peers)
        {
            foreach (var peer in peers)
                Append(peer);
        }


        public void Remove(Address peer)
        {
            lock (_peers)
            {
                _peers.Remove(peer);
            }
        }

        public void Remove(Address[] peers)
        {
            foreach (var peer in peers)
                Remove(peer);
        }

        public void Remove(ElasticAddress[] peers)
        {
            foreach (var peer in peers)
                Remove(peer);
        }

        public void Remove(ElasticAddress peer)
        {
            Remove(peer.Address);
        }

        public void Remove(string[] peers)
        {
            foreach (var peer in peers)
                Remove(peer);
        }

        public void Remove(string peer)
        {
            Remove(new Address(peer));
        }


        public void Clear()
        {
            lock (_peers)
            {
                _peers.Clear();
            }
        }

        public void Update(string[] peers)
        {
            lock (_peers)
            {
                _peers.Clear();
                Append(peers);
            }
        }


        public ElasticAddress Find(string peer)
        {
            return Find(new Address(peer));
        }

        public ElasticAddress Find(Address peer)
        {
            lock (_peers)
                return _peers.ContainsKey(peer) ? _peers[peer] : null;
        }

        public bool Exists(string peer)
        {
            return Exists(new Address(peer));
        }

        public bool Exists(Address peer)
        {
            lock (_peers)
                return _peers.ContainsKey(peer);
        }

        public bool Exists(ElasticAddress peer)
        {
            return Exists(peer.Address);
        }

        public bool Exists(string host, int port)
        {
            foreach (var peer in ToArray<ElasticAddress>())
                if (peer.Host == host && peer.Port == port) return true;

            return false;
        }

        public T[] ToArray<T>()
        {
            lock( _peers )
                return ToArray<T>(_peers.Values.ToArray());
        }

        public T[] ToArray<T>(ElasticAddress[] peers)
        {
            if (typeof(T) == typeof(ElasticAddress))
                return peers as T[];

            if (typeof(T) == typeof(string))
            {
                List<string> lists = new List<string>();
                foreach (var peer in peers)
                    lists.Add(peer.ToString());

                return lists.ToArray() as T[];
            }

            if (typeof(T) == typeof(Address))
            {
                List<Address> lists = new List<Address>();
                foreach (var peer in peers)
                    lists.Add(peer.Address);

                return lists.ToArray() as T[];
            }

            throw new TypeLoadException("unsupported type");
        }


        public T[] ToArray<T>(Elastic3D coordinates, ElasticLayout layout)
        {
            List<ElasticAddress> resultPeers = new List<ElasticAddress>();
            foreach (var peer in ToArray<ElasticAddress>())
            {
                Elastic3D pos = layout.DefineCoordinates(peer.Address);
                if (coordinates.Contains(pos))
                    resultPeers.Add(peer);
            }

            return ToArray<T>(resultPeers.ToArray());
        }

        //public byte[] ComputeMerkleHash(string[] peers)
        //{
        //    if (peers.Length == 0) return new byte[32];

        //    // initialize merkle tree
        //    List<byte[]> merkle = new List<byte[]>();
        //    foreach (var peer in peers)
        //        merkle.Add(new ElasticAddress(peer).Hash);

        //    // process for root hash
        //    while (merkle.Count > 1)
        //    {
        //        if (merkle.Count % 2 != 0)
        //            merkle.Add(merkle.Last());

        //        List<byte[]> new_merkle = new List<byte[]>();
        //        for (int i = 0; i < merkle.Count; i += 2)
        //            new_merkle.Add(HashProvider.Hash256(merkle[i].Merge(merkle[i + 1])));

        //        merkle = new_merkle;
        //    }

        //    return merkle.Count == 1 ? merkle[0] : new byte[32];
        //}

    }
}
