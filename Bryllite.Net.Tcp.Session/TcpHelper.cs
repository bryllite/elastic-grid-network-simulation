using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace Bryllite.Net.Tcp
{
    public class TcpHelper
    {
        public static readonly string ANY = "0.0.0.0";

        public static IPEndPoint ToIPEndPoint(string host, int port)
        {
            return new IPEndPoint(ToIPAddress(host), port);
        }

        public static IPAddress ToIPAddress(string host)
        {
            return ( string.IsNullOrEmpty(host) || 0 == host.CompareTo(ANY) ) ? IPAddress.Any : Dns.GetHostAddresses(host)[0];
        }
    }
}
