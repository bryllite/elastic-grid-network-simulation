using BrylliteLib.Net;
using System;
using System.Collections.Generic;
using System.Text;

namespace NodeServerApp
{
    public class AppConfig
    {
        public string Host;

        public string TrackerHost;
        public int TrackerPort;

        public bool Redirect;

        public AppConfig()
        {
            Host = TCPHelper.IP_ANY;

            TrackerHost = "127.0.0.1";
            TrackerPort = TCPHelper.TRACKER_PORT;

            Redirect = false;
        }
    }
}
