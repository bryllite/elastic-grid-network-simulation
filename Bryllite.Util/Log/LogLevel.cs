using System;
using System.Collections.Generic;
using System.Text;

namespace Bryllite.Util.Log
{
    public enum LogLevel
    {
        ERROR = 0x01,
        WARNING = 0x02,
        DEBUG = 0x04,
        TRACE = 0x08,
        INFO = 0x10,
        VERB = 0x20
    }
}
