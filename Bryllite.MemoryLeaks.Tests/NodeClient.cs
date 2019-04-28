using System;
using System.Collections.Generic;
using System.Text;
using Bryllite.Net.Tcp;
using Bryllite.Util.Log;

namespace Bryllite.MemoryLeaks.Tests
{
    public class NodeClient
    {
        private ILoggable Logger;
        private TcpClient _tcp_client;

        public NodeClient(ILoggable logger)
        {
            Logger = logger;

            _tcp_client = new TcpClient( logger )
            {
            };
        }

        public void Start(string host, int port)
        {
            _tcp_client.Start(host, port);
        }

        public void Stop()
        {
            _tcp_client.Stop();
        }

        public int Write(byte[] data)
        {
            return _tcp_client.Send(data);
        }
    }
}
