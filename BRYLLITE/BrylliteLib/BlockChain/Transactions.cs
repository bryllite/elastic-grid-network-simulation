using BrylliteLib.Hash;
using BrylliteLib.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BrylliteLib.BlockChain
{
    public class Transactions
    {
        private Dictionary<byte[], Transaction> _transactions = new Dictionary<byte[], Transaction>();

        public int Count
        {
            get
            {
                return _transactions.Count;
            }
        }

        public Transactions()
        {
        }

        private Transactions(byte[] bytes)
        {
            int len = BitConverter.ToInt32(bytes, 0);
            for (int i = 0; i < len; i++)
                Append(Transaction.FromBytes(bytes.Skip(4 + (i * Transaction.TX_BYTES)).Take(Transaction.TX_BYTES).ToArray()));
        }

        public static Transactions FromBytes(byte[] bytes)
        {
            return new Transactions(bytes);
        }

        public byte[] ToBytes()
        {
            List<byte> bytes = new List<byte>();
            Transaction[] txs = _transactions.Values.ToArray();
            bytes.AddRange(BitConverter.GetBytes(_transactions.Count));
            for (int i = 0; i < txs.Length; i++)
                bytes.AddRange(txs[i].ToBytes());

            return bytes.ToArray();
        }


        public byte[] ToByteArray()
        {
            return ToBytes();
        }

        public Transaction[] ToArray()
        {
            return _transactions.Values.ToArray();
        }

        public void Append(Transaction tx)
        {
            lock (_transactions)
            {
                if (!Exists(tx.TxID) /* && tx.Verify() */ )
                    _transactions.Add(tx.TxID, tx);
            }
        }

        public void Append(Transactions txs)
        {
            Transaction[] transactions;
            lock (txs._transactions)
            {
                transactions = txs._transactions.Values.ToArray();
            }

            foreach (Transaction tx in transactions)
                Append(tx);
        }

        public void Remove(byte[] txid)
        {
            if (Exists(txid))
            {
                lock (_transactions)
                {
                    _transactions.Remove(txid);
                }
            }
        }

        public Transaction Find(byte[] txid)
        {
            lock (_transactions)
            {
                foreach (byte[] id in _transactions.Keys.ToArray())
                {
                    if (txid.SequenceEqual(id))
                        return _transactions[id];
                }
            }

            return null;
        }

        public bool Exists(byte[] txid)
        {
            lock (_transactions)
            {
                return _transactions.ContainsKey(txid);
            }
        }

        public bool Verify()
        {
            Transaction[] txs;
            lock (_transactions)
            {
                txs = _transactions.Values.ToArray();
            }

            foreach (var tx in txs)
            {
                if (!tx.Verify())
                    return false;
            }

            return true;
        }

        public Transactions Shuffle()
        {
            Transactions txs = new Transactions();
            Transaction[] arr;
            lock (_transactions)
            {
                arr = _transactions.Values.OrderBy(x => RndGenerator.GetUInt()).ToArray();
            }

            foreach (var tx in arr)
                txs.Append(tx);

            return txs;
        }

        public byte[] ComputeMerkleTreeHash()
        {
            lock (_transactions)
            {
                if (_transactions.Count == 0) return new byte[32];

                List<byte[]> merkle = new List<byte[]>();
                foreach (Transaction tx in _transactions.Values.ToArray())
                    merkle.Add(tx.TxID);

                while (merkle.Count > 1)
                {
                    // copy last txid if odd number
                    if (merkle.Count % 2 != 0)
                        merkle.Add(merkle.Last());

                    // create new merkle tree with given merkle
                    List<byte[]> new_merkle = new List<byte[]>();
                    for (int i = 0; i < merkle.Count; i += 2)
                    {
                        byte[] prev = merkle[i];
                        byte[] next = merkle[i + 1];

                        List<byte> bytes = new List<byte>(prev);
                        bytes.AddRange(next);

                        new_merkle.Add(HashUtil.Hash256(bytes.ToArray()));
                    }

                    // need to save merkle tree here for each step ( 


                    merkle = new_merkle;
                }

                return merkle.Count == 1 ? merkle[0] : new byte[32];
            }
        }
    }
}
