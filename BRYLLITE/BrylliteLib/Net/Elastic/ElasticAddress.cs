using BrylliteLib.Crypto;
using BrylliteLib.Utils;
using System;
using System.Collections.Generic;
using System.Text;

namespace BrylliteLib.Net.Elastic
{
    // 엘라스틱 노드 주소
    // enode://Address@Host:Port
    public class ElasticAddress
    {
        public static readonly string PREFIX = "enode://";
        public static readonly char[] SEPERATORS = { '@', ':' };

        // 노드 계좌 주소
        private CAddress _address;

        // 노드 public address ( ip or domain )
        private string _host;

        // 노드 포트
        private int _port;

        public CAddress Address
        {
            get
            {
                return _address;
            }
            private set
            {
                _address = value;
            }
        }

        public string HexAddress
        {
            get
            {
                return Address.HexAddress;
            }
        }

        public string Host
        {
            get
            {
                return _host;
            }
            private set
            {
                _host = value;
            }
        }

        public int Port
        {
            get
            {
                return _port;
            }
            private set
            {
                _port = value;
            }
        }

        public ElasticAddress(string eNodeID)
        {
            eNodeID = eNodeID.Replace(PREFIX, "");
            string[] values = eNodeID.RemoveHexPrefix().Split(SEPERATORS);

            try
            {
                Address = values[0].HexToByteArray();
                Host = values[1];
                Port = Convert.ToInt32(values[2]);
            }
            catch (Exception e)
            {
                throw new ArgumentException("Invalid ENodeID format", e);
            }
        }

        public ElasticAddress(CAddress address, string host, int port)
        {
            Address = address;
            Host = host;
            Port = port;
        }

        public override string ToString()
        {
            return $"{PREFIX}{HexAddress}@{Host}:{Port}";
        }

        public static implicit operator string(ElasticAddress b)
        {
            return b.ToString();
        }

        public static implicit operator ElasticAddress(string s)
        {
            return new ElasticAddress(s);
        }

    }
}
