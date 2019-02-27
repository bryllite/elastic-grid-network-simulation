using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace BrylliteLib.Net
{
    public static class TCPHelper
    {
        public static readonly string IP_ANY = "0.0.0.0";

        // 트래커 서비스 포트
        public static readonly int TRACKER_PORT = 19999;

        public static IPEndPoint ToIPEndPoint(string host, int port)
        {
            return new IPEndPoint((string.IsNullOrEmpty(host) || 0 == host.CompareTo(IP_ANY)) ? IPAddress.Any : Dns.GetHostAddresses(host)[0], port);
        }
    }
}
