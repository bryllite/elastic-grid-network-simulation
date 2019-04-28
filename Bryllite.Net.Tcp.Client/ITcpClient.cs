using System;
using System.Collections.Generic;
using System.Text;

namespace Bryllite.Net.Tcp
{
    public interface ITcpClient
    {
        ITcpSession Session { get; }

        bool Connected { get; }

        TcpClient.ConnectState State { get; }

        void Start(string host, int port, int session_read_buffer_length = 0);

        void Stop();

        int Send(byte[] data);

    }
}
