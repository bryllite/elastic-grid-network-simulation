using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace Bryllite.Util.Log
{
    public class FileLogger : ILoggable
    {
        public LogLevel mLogFilter { get; set; }

        public string LogFilePath { get; set; }

        public virtual string GetLogFilePath()
        {
            return LogFilePath;
        }

        public FileLogger(string logFilePath) : this(logFilePath, LogFilter.None)
        {
        }

        public FileLogger(string logFilePath, LogLevel logFilter)
        {
            LogFilePath = logFilePath;

            string path = Path.GetDirectoryName(LogFilePath);
            if ( !string.IsNullOrEmpty(path) && !Directory.Exists(path))
                Directory.CreateDirectory(path);

            mLogFilter = logFilter;
        }

        private void WriteToFile(string message)
        {
            try
            {
                lock (this)
                {
                    File.AppendAllText(GetLogFilePath(), message);
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine($"{GetType().Name}.Write() exception! e={e.Message}");
            }
        }

        public void LogWrite(LogLevel logLevel, string tag, string message)
        {
            if (string.IsNullOrEmpty(tag))
            {
                LogWrite(logLevel, message);
                return;
            }

            if (!mLogFilter.Match(logLevel)) return;
            WriteToFile($"[{LogExtension.TimeCode}] [{logLevel.ToString()}] <{tag}> {message}{LogExtension.CRLF}");
        }

        public void LogWrite(LogLevel logLevel, string message)
        {
            if (!mLogFilter.Match(logLevel)) return;
            WriteToFile($"[{LogExtension.TimeCode}] [{logLevel.ToString()}] {message}{LogExtension.CRLF}");
        }

        public void LogWrite(string message)
        {
            WriteToFile($"[{LogExtension.TimeCode}] {message}{LogExtension.CRLF}");
        }
    }
}