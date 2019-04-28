using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using Bryllite.Net.Elastic;
using Bryllite.Net.Tcp;
using Bryllite.Util.Log;

namespace Bryllite.MemoryLeaks.Tests
{
    public class NodeServer
    {
        private ILoggable Logger;
        private TcpServer _tcp_server;

        public NodeServer( ILoggable logger )
        {
            Logger = logger;

            _tcp_server = new TcpServer( logger )
            {
                OnAccept = OnAccept,
                OnClose = OnClose,
                OnMessage = OnMessage
            };
        }

        private CancellationTokenSource cts;
        public void Start( int port, CancellationTokenSource cts )
        {
            this.cts = cts;
            _tcp_server.Start(null, port, 1);
        }

        public void Stop()
        {
            _tcp_server.Stop();
            cts.Cancel();
        }

        public void OnAccept(ITcpSession session)
        {
            Logger.debug("new connection");
        }

        public void OnClose(ITcpSession session, int reason)
        {
            Logger.debug("connection lost");
        }

        public void OnMessage(ITcpSession session, byte[] data)
        {
            Logger.debug($"message received data.length={data.Length}");
        }
    }
}
