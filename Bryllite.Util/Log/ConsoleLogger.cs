using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Bryllite.Util.Log
{
    public class ConsoleLogger : ILoggable
    {
        public LogLevel mLogFilter
        {
            get
            {
                return ReferenceEquals(Instance, null) ? LogFilter.None : Instance.mLogFilter ;
            }
            set
            {
                if (!ReferenceEquals(Instance, null)) Instance.mLogFilter = value;
            }
        }

        public class ConsoleLoggerInternal : ILoggable
        {
            public LogLevel mLogFilter { get; set; } = LogFilter.Default;

            public string TimeCode => LogExtension.TimeCode;

            public void LogWrite(LogLevel logLevel, string tag, string message)
            {
                if (!mLogFilter.Match(logLevel)) return;

                string msg = $"{tag} {message}";
                switch (logLevel)
                {
                    case LogLevel.ERROR: BConsole.Error(msg); break;
                    case LogLevel.WARNING: BConsole.Warning(msg); break;
                    case LogLevel.DEBUG: BConsole.Debug(msg); break;
                    case LogLevel.TRACE: BConsole.Trace(msg); break;
                    case LogLevel.INFO: BConsole.Info(msg); break;
                    case LogLevel.VERB: BConsole.Verb(msg); break;
                    default: break;
                }
            }

            public void LogWrite(LogLevel logLevel, string message)
            {
                if (!mLogFilter.Match(logLevel)) return;

                switch (logLevel)
                {
                    case LogLevel.ERROR: BConsole.Error(message); break;
                    case LogLevel.WARNING: BConsole.Warning(message); break;
                    case LogLevel.DEBUG: BConsole.Debug(message); break;
                    case LogLevel.TRACE: BConsole.Trace(message); break;
                    case LogLevel.INFO: BConsole.Info(message); break;
                    case LogLevel.VERB: BConsole.Verb(message); break;
                    default: break;
                }
            }

            public void LogWrite(string message)
            {
                BConsole.Verb(message);
            }
        }

        public static ConsoleLoggerInternal _instance = null;
        public static ConsoleLoggerInternal Instance
        {
            get
            {
                if (ReferenceEquals(_instance, null)) _instance = new ConsoleLoggerInternal();
                return _instance;
            }
        }

        public ConsoleLogger()
        {
        }

        public ConsoleLogger( LogLevel filter) : this()
        {
            Instance.mLogFilter = filter;
        }

        public void LogWrite(LogLevel logLevel, string tag, string message )
        {
            Instance.LogWrite(logLevel, tag, message);
        }

        public void LogWrite(LogLevel logLevel, string message)
        {
            Instance.LogWrite(logLevel, message);
        }

        public void LogWrite(string message)
        {
            Instance.LogWrite(message);
        }

        internal static ConsoleColor GetLogLevelColor(LogLevel logLevel)
        {
            switch (logLevel)
            {
                case LogLevel.ERROR: return ConsoleColor.DarkRed;
                case LogLevel.WARNING: return ConsoleColor.DarkYellow;
                case LogLevel.DEBUG: return ConsoleColor.Gray;
                case LogLevel.TRACE: return ConsoleColor.Cyan;
                case LogLevel.INFO: return ConsoleColor.White;
                case LogLevel.VERB: return ConsoleColor.Blue;
                default: break;
            }

            return ConsoleColor.Gray;
        }
    }
}
