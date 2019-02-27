using BrylliteLib.Crypto;
using BrylliteLib.Hash;
using BrylliteLib.Utils;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;

namespace BrylliteLib.BlockChain
{
    public class BlockHeader
    {
        public static readonly long INVALID_BLOCK = -1;
        public static readonly int VERSION = 0x01;
        public static readonly int NONCE_LENGTH = 8;
        public static readonly int ADDRESS_LENGTH = CPublicKey.ADDRESS_BYTES;

        public long Height = INVALID_BLOCK;
        public int Version = VERSION;
        public long TimeStamp = DateTime.Now.Ticks;
        public byte[] PrevBlockHash;
        public byte[] TxMerkleRootHash;

        public byte[] Nonce = new byte[NONCE_LENGTH];
        public byte[] UserAddress = new byte[ADDRESS_LENGTH];

        private BlockHeader()
        {
            Height = INVALID_BLOCK;
            Version = VERSION;
            TimeStamp = DateTime.Now.Ticks;
            PrevBlockHash = new byte[32];
            TxMerkleRootHash = new byte[32];

            Nonce = new byte[NONCE_LENGTH];
            UserAddress = new byte[ADDRESS_LENGTH];
        }

        public BlockHeader(long height) : this()
        {
            Height = height;
        }

        public BlockHeader(long height, byte[] prevBlockHash) : this(height)
        {
            SetPrevBlockHash(prevBlockHash);
        }

        public BlockHeader(byte[] bytes) : this()
        {
            Version = BitConverter.ToInt32(bytes, 0);
            Height = BitConverter.ToInt64(bytes, 4);
            TimeStamp = BitConverter.ToInt64(bytes, 12);
            Buffer.BlockCopy(bytes, 20, PrevBlockHash, 0, 32);
            Buffer.BlockCopy(bytes, 52, TxMerkleRootHash, 0, 32);
            Buffer.BlockCopy(bytes, 84, Nonce, 0, NONCE_LENGTH);
            Buffer.BlockCopy(bytes, 92, UserAddress, 0, ADDRESS_LENGTH);
        }

        public byte[] ToByteArray()
        {
            List<byte> bytes = new List<byte>();

            bytes.AddRange(BitConverter.GetBytes(Version));
            bytes.AddRange(BitConverter.GetBytes(Height));
            bytes.AddRange(BitConverter.GetBytes(TimeStamp));
            bytes.AddRange(PrevBlockHash);
            bytes.AddRange(TxMerkleRootHash);
            bytes.AddRange(Nonce);
            bytes.AddRange(UserAddress);

            return bytes.ToArray();
        }

        public void SetNonce(byte[] nonce)
        {
            if (nonce.Length != NONCE_LENGTH)
                throw new ArgumentException("Invalid nonce length");

            Buffer.BlockCopy(nonce, 0, Nonce, 0, NONCE_LENGTH);
        }

        public void SetUserAddress(byte[] address)
        {
            if (address.Length != ADDRESS_LENGTH)
                throw new ArgumentException("Invalid address length");

            Buffer.BlockCopy(address, 0, UserAddress, 0, ADDRESS_LENGTH);
        }

        public void SetUserAddress(string address)
        {
            SetUserAddress(address.RemoveHexPrefix().HexToByteArray());
        }

        public void SetMerkleRootHash(byte[] hash)
        {
            if (hash.Length != 32)
                throw new ArgumentException("Invalid MerkleRootHash length");

            Buffer.BlockCopy(hash, 0, TxMerkleRootHash, 0, 32);
        }

        public void SetPrevBlockHash(byte[] hash)
        {
            if (hash.Length != 32)
                throw new ArgumentException("Invalid PrevBlockHash length");

            Buffer.BlockCopy(hash, 0, PrevBlockHash, 0, 32);
        }

        public byte[] Hash
        {
            get
            {
                return HashUtil.Hash256(ToByteArray());
            }
        }

        public override string ToString()
        {
            JObject json = new JObject();

            json.Add("Height", Height);
            json.Add("Version", Version);
            json.Add("TimeStamp", TimeStamp);
            json.Add("PrevBlockHash", PrevBlockHash.ToHexString());
            json.Add("TxMerkleRootHash", TxMerkleRootHash.ToHexString());
            json.Add("Nonce", Nonce.ToHexString());
            json.Add("UserAddress", "0x" + UserAddress.ToHexString());
            json.Add("Hash", Hash.ToHexString());

            return json.ToString();
        }

        public static bool operator <(BlockHeader a, BlockHeader b)
        {
            return a.Hash.ToHexString().CompareTo(b.Hash.ToHexString()) < 0;
        }

        public static bool operator >(BlockHeader a, BlockHeader b)
        {
            return a.Hash.ToHexString().CompareTo(b.Hash.ToHexString()) > 0;
        }
    }
}
