using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Bryllite.Util.Log
{
    public class RotateFileLogger : FileLogger
    {
        public RotateFileLogger(string logFilePath, LogLevel logFilter) : base(logFilePath, logFilter)
        {
        }

        public RotateFileLogger(string logFilePath) : base(logFilePath, LogFilter.None)
        {
        }

        public override string GetLogFilePath()
        {
            string path = Path.GetDirectoryName(LogFilePath);
            string logFileName = $"{Path.GetFileNameWithoutExtension(LogFilePath)}-{LogExtension.DateCode}{Path.GetExtension(LogFilePath)}";
            return string.IsNullOrEmpty(path) ? logFileName : $"{path}/{logFileName}";
        }
    }
}
