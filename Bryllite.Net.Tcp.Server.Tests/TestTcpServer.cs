using System;
using System.Collections.Generic;
using System.Text;
using Bryllite.Util.Log;

namespace Bryllite.Net.Tcp.Server.Tests
{
    public class TestTcpServer
    {
        private ILoggable Logger;
        private TcpServer TcpServer;

        public TestTcpServer( ILoggable logger )
        {
            Logger = logger;

            TcpServer = new TcpServer(logger)
            {
                OnStart = OnStart,
                OnStop = OnStop,
                OnAccept = OnAccept,
                OnClose = OnClose,
                OnMessage = OnMessage,
                OnWrite = OnWrite
            };

        }

        public void Start(string host, int port)
        {
            TcpServer.Start(host, port, 16);
        }

        public void Stop()
        {
            TcpServer.Stop();
        }

        public void OnStart(string host, int port)
        {
            Logger.debug($"TcpServer started on {host}:{port}");
        }

        public void OnStop()
        {
            Logger.debug("TcpServer stop");
        }

        public void OnAccept(ITcpSession session)
        {
            Logger.debug($"New connection! sid={session.SID}");
        }

        public void OnClose(ITcpSession session, int reason)
        {
            Logger.debug($"Connection lost! sid={session.SID}");
        }

        public void OnMessage(ITcpSession session, byte[] data)
        {
            Logger.debug($"Received message! data.length={data.Length}");
        }

        public void OnWrite(ITcpSession session, byte[] data)
        {
            Logger.debug($"Write complete! writeBytes={data.Length}");
        }
    }
}
