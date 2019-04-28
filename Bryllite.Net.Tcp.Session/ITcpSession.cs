using System;
using System.Collections.Generic;
using System.Text;

namespace Bryllite.Net.Tcp
{
    public interface ITcpSession 
    {
        ulong SID { get; }

        string Remote { get; }

        bool Connected { get; }

        void Start();

        void Stop(int reason = 0);

        int Write(byte[] data);

    }
}
