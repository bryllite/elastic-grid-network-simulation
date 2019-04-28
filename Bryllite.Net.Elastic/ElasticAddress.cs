using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Bryllite.Core.Hash;
using Bryllite.Core.Key;

namespace Bryllite.Net.Elastic
{
    public class ElasticAddress
    {
        public static readonly string PREFIX = "enode://";
        public static readonly char[] SEPERATORS = { '@', ':' };

        // account address
        public Address Address { get; private set; }

        // host url
        public string Host { get; private set; }

        // host port
        public int Port { get; private set; }

        public string HexAddress => Address.HexAddress;

        public byte[] Hash
        {
            get
            {
                List<byte> bytes = new List<byte>();

                bytes.AddRange(Address.Bytes);
                bytes.AddRange(Encoding.UTF8.GetBytes(Host));
                bytes.AddRange(BitConverter.GetBytes(Port));

                return HashProvider.Hash256(bytes.ToArray());
            }
        }

        public ElasticAddress(Address address, string host, int port)
        {
            Address = address;
            Host = host;
            Port = port;
        }

        public ElasticAddress(string enode)
        {
            try
            {
                string[] tokens = enode.Replace(PREFIX, "").Split(SEPERATORS);

                Address = new Address(tokens[0]);
                Host = tokens[1];
                Port = Convert.ToInt32(tokens[2]);
            }
            catch (Exception e)
            {
                throw new FormatException("enode format exception", e);
            }
        }

        public static ElasticAddress FromString(string enode)
        {
            return new ElasticAddress(enode);
        }

        public override string ToString()
        {
            return $"{PREFIX}{HexAddress}{SEPERATORS[0]}{Host}{SEPERATORS[1]}{Port}";
        }

        public override int GetHashCode()
        {
            return Address.GetHashCode() + Host.GetHashCode() + Port.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            var o = obj as ElasticAddress;
            return (!ReferenceEquals(o, null) && Address == o.Address && Host.Equals(o.Host, StringComparison.OrdinalIgnoreCase) && Port == o.Port);
        }

        public static bool operator ==(ElasticAddress left, ElasticAddress right)
        {
            if (ReferenceEquals(left, null)) return ReferenceEquals(right, null);
            return left.Equals(right);
        }

        public static bool operator !=(ElasticAddress left, ElasticAddress right)
        {
            return !(left == right);
        }

        public static implicit operator ElasticAddress(string s)
        {
            return new ElasticAddress(s);
        }
    }
}
