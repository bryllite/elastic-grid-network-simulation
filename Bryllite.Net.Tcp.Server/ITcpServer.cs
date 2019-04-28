using System;
using System.Collections.Generic;
using System.Text;

namespace Bryllite.Net.Tcp
{
    public interface ITcpServer
    {
        bool Running { get; }

        bool Start(string host, int port, int acceptThreadCount, int backlogs = 32);

        void Stop();

        void SendTo(ITcpSession session, byte[] data);

        void SendAll(byte[] data);
    }
}
