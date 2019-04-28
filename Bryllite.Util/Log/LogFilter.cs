using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Bryllite.Util.Log
{
    public static class LogFilter
    {
        public static readonly LogLevel None;
        public static readonly LogLevel All = LogLevel.ERROR | LogLevel.WARNING | LogLevel.DEBUG | LogLevel.TRACE | LogLevel.INFO | LogLevel.VERB;
        public static readonly LogLevel Production = LogLevel.ERROR | LogLevel.WARNING | LogLevel.INFO;
        public static readonly LogLevel Debug = All;

        public static LogLevel Default = Debugger.IsAttached ? Debug : Production;

        public static bool Match(this LogLevel filter, LogLevel level)
        {
            if (ReferenceEquals(filter, null) || ReferenceEquals(filter, None) || ReferenceEquals(level, null) ) return false;

            return (filter & level) != 0 ;
        }
    }
}
