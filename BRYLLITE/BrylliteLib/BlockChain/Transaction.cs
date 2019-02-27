using BrylliteLib.Crypto;
using BrylliteLib.Hash;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BrylliteLib.BlockChain
{
    public class Transaction
    {
        public static readonly int TX_BYTES = 138;
        public static readonly byte VERSION = 0x01;

        private byte _version = VERSION;
        private byte _reserved = 0;
        private DateTime _time_stamp = DateTime.Now;
        private long _value = 0;
        private long _fee = 0;

        private CAddress _sender = null;
        private CAddress _receiver = null;

        private CSignature _owner_sign = null;

        public byte Version => _version;
        public bool CoinBase
        {
            get
            {
                return _sender == (CAddress)CAddress.Null;
            }
        }

        public DateTime TimeStamp => _time_stamp;
        public long Value => _value;
        public long Fee => _fee;

        public CAddress Sender => _sender;
        public CAddress Receiver => _receiver;
        public CSignature OwnerSign => _owner_sign;

        public byte[] Hash
        {
            get
            {
                return HashUtil.Hash256(ToBytes(false));
            }
        }

        public byte[] TxID
        {
            get
            {
                return HashUtil.Hash256(ToBytes(true));
            }
        }

        public Transaction()
        {
        }

        public Transaction(CAddress to, long value) : this(CAddress.Null, to, value)
        {
        }

        public Transaction(CAddress from, CAddress to, long value)
        {
            _sender = from;
            _receiver = to;
            _value = value;
        }

        private Transaction(byte[] bytes)
        {
            _version = bytes[0];
            _reserved = bytes[1];
            _time_stamp = new DateTime(BitConverter.ToInt64(bytes, 2));
            _value = BitConverter.ToInt64(bytes, 10);
            _fee = BitConverter.ToInt64(bytes, 18);
            _sender = bytes.Skip(26).Take(CAddress.ADDRESS_BYTES).ToArray();
            _receiver = bytes.Skip(46).Take(CAddress.ADDRESS_BYTES).ToArray();
            _owner_sign = CSignature.FromByteArray(bytes.Skip(66).Take(CSignature.SIGNATURE_BYTES).ToArray());
        }

        private byte[] ToBytes(bool withSignature)
        {
            List<byte> bytes = new List<byte>();

            bytes.Add(_version);
            bytes.Add(_reserved);
            bytes.AddRange(BitConverter.GetBytes(_time_stamp.Ticks));
            bytes.AddRange(BitConverter.GetBytes(_value));
            bytes.AddRange(BitConverter.GetBytes(_fee));
            bytes.AddRange((byte[])_sender);
            bytes.AddRange((byte[])_receiver);

            if (withSignature)
                bytes.AddRange(_owner_sign.ToByteArray());

            return bytes.ToArray();
        }

        public byte[] ToBytes()
        {
            return ToBytes(true);
        }

        public static Transaction FromBytes(byte[] bytes)
        {
            return new Transaction(bytes);
        }


        public void Sign(CPrivateKey ownerPrivateKey)
        {
            _owner_sign = ownerPrivateKey.Sign(Hash);
        }


        public bool Verify()
        {
            try
            {
                byte[] hash = Hash;
                CPublicKey publicKey = _owner_sign.ToPublicKey(hash);
                return publicKey.Verify(_owner_sign, hash) && publicKey.HexAddress == _sender.HexAddress;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
