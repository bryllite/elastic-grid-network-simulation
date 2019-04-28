using System;
using System.Collections.Generic;
using System.Text;

namespace Bryllite.Util.Log
{
    public interface ILoggable
    {
        LogLevel mLogFilter { get; set; }

        void LogWrite(LogLevel logLevel, string tag, string message);

        void LogWrite(LogLevel logLevel, string message);

        void LogWrite(string message);

    }
}
