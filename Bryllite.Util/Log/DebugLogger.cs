using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Bryllite.Util.Log
{
    public class DebugLogger : ILoggable
    {
        private static DebugLoggerInternal _instance = null;
        public static DebugLoggerInternal Instance
        {
            get
            {
                if (ReferenceEquals(_instance, null)) _instance = new DebugLoggerInternal();
                return _instance;
            }
        }

        public LogLevel mLogFilter
        {
            get
            {
                return ReferenceEquals(Instance, null) ? LogFilter.None : Instance.mLogFilter;
            }
            set
            {
                if (!ReferenceEquals(Instance, null)) Instance.mLogFilter = value;
            }
        }


        public class DebugLoggerInternal : ILoggable
        {
            public LogLevel mLogFilter { get; set; } = LogFilter.Default;

            public void LogWrite(LogLevel logLevel, string tag, string message)
            {
                if (string.IsNullOrEmpty(tag))
                {
                    LogWrite(logLevel, message);
                    return;
                }

                if (!mLogFilter.Match(logLevel)) return;

                lock (this)
                {
                    Debug.WriteLine($"[{LogExtension.TimeCode}] [{logLevel.ToString()}] <{tag}> {message}");
                }
            }

            public void LogWrite(LogLevel logLevel, string message)
            {
                if (!mLogFilter.Match(logLevel)) return;

                lock (this)
                {
                    Debug.WriteLine($"[{LogExtension.TimeCode}] [{logLevel.ToString()}] {message}");
                }
            }

            public void LogWrite(string message)
            {
                Debug.WriteLine($"[{LogExtension.TimeCode}] {message}");
            }
        }

        public DebugLogger()
        {
        }

        public DebugLogger(LogLevel filter) : this()
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
    }
}
