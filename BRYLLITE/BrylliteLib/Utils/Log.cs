using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;

namespace BrylliteLib.Utils
{
    public class Log
    {
        public enum LogType
        {
            ERROR,
            WARNING,
            DEBUG,
            INFO,
            VERB,
            USER
        }

        private static object self = new object();
        public static bool LogToFile;

        static Log()
        {
            LogToFile = false;
        }

        public static void SetLogFilePath(string logFileName)
        {
            string path = Path.GetDirectoryName(logFileName);
            if (!string.IsNullOrEmpty(path) && !Directory.Exists(path))
                Directory.CreateDirectory(path);

            LogFile = logFileName;
            LogToFile = true;
        }

        public static string TimeStamp
        {
            get
            {
                return $"{DateCode} {TimeCode}";
            }
        }

        public static string TimeCode
        {
            get
            {
                return DateTime.Now.ToString("HH:mm:ss.fff");
            }
        }

        public static string DateCode
        {
            get
            {
                return DateTime.Now.ToString("yyyy-MM-dd");
            }
        }

        private static string mLogFile = "Log.log";
        public static string LogFile
        {
            get
            {
                return mLogFile;
            }
            private set
            {
                mLogFile = value;
            }
        }

        public static void e(string tag, string msg)
        {
            lock (self)
            {
                LogType logType = LogType.ERROR;
                string timeStamp = TimeCode;

                WriteToConsole(logType, timeStamp, tag, msg, ConsoleColor.Red);
                WriteToFile(logType, timeStamp, tag, msg);
            }

            throw new Exception(msg);
        }

        public static void w(string tag, string msg)
        {
            lock (self)
            {
                LogType logType = LogType.WARNING;
                string timeStamp = TimeCode;

                WriteToConsole(logType, timeStamp, tag, msg, ConsoleColor.Yellow);
                WriteToFile(logType, timeStamp, tag, msg);
            }
        }

        public static void d(string tag, string msg)
        {
            lock (self)
            {
                LogType logType = LogType.DEBUG;
                string timeStamp = TimeCode;

                WriteToConsole(logType, timeStamp, tag, msg, ConsoleColor.Gray);
                WriteToFile(logType, timeStamp, tag, msg);
            }
        }

        public static void i(string tag, string msg)
        {
            lock (self)
            {
                LogType logType = LogType.INFO;
                string timeStamp = TimeCode;

                WriteToConsole(logType, timeStamp, tag, msg, ConsoleColor.Gray);
                WriteToFile(logType, timeStamp, tag, msg);
            }
        }
        public static void v(string tag, string msg)
        {
            lock (self)
            {
                LogType logType = LogType.VERB;
                string timeStamp = TimeCode;

                WriteToConsole(logType, timeStamp, tag, msg, ConsoleColor.Gray);
                WriteToFile(logType, timeStamp, tag, msg);
            }
        }

        public static void u(string tag, string msg)
        {
            lock (self)
            {
                LogType logType = LogType.USER;
                string timeStamp = TimeCode;

                WriteToConsole(logType, timeStamp, tag, msg, ConsoleColor.Gray);
                WriteToFile(logType, timeStamp, tag, msg);
            }
        }

        public static void e(string msg)
        {
            e(null, msg);
        }

        public static void w(string msg)
        {
            w(null, msg);
        }

        public static void d(string msg)
        {
            d(null, msg);
        }

        public static void i(string msg)
        {
            i(null, msg);
        }

        public static void v(string msg)
        {
            v(null, msg);
        }

        public static void u(string msg)
        {
            u(null, msg);
        }

        private static string CallStack(int frame)
        {
            var stackTrace = new StackTrace();
            var stackFrameMethod = stackTrace.GetFrame(frame).GetMethod();

            return $"{stackFrameMethod.ReflectedType.Name}::{stackFrameMethod.Name}";
        }


        private static string[] CallStacks
        {
            get
            {
                List<string> listCallStacks = new List<string>();

                var stackTrace = new StackTrace();
                var stackFrames = stackTrace.GetFrames();

                foreach (var frame in stackFrames)
                {
                    var frameMethod = frame.GetMethod();

                    string className = frameMethod.ReflectedType.Name;
                    string methodName = frameMethod.Name;

                    if (className != MethodBase.GetCurrentMethod().DeclaringType.Name.ToString())
                        listCallStacks.Add($"{className}::{methodName}");
                }

                return listCallStacks.ToArray();
            }

        }

        public static void WriteToFile(LogType logType, string timeStamp, string tag, string msg)
        {
            if (!LogToFile && logType > LogType.WARNING) return;

            string t = tag != null ? $"#{tag}" : "";

            string output = $"[{timeStamp}]:{logType.ToString()}/> {t}: {msg}\r\n";
            string callstack = $"CallStack=({string.Join("\r\n", CallStacks)})";

            try
            {
                File.AppendAllText(LogFile, output);
                if (logType <= LogType.WARNING)
                    File.AppendAllText(LogFile, callstack);
            }
            catch (Exception e)
            {
                Console.WriteLine("Log.WriteToFile(): Exception! e.Message=" + e.Message);
            }
        }

        private static string LogTypeToString(LogType logType)
        {
            return logType.ToString();
        }

        private static void WriteToConsole(LogType logType, string timeStamp, string tag, string msg, ConsoleColor msgColor = ConsoleColor.Gray, ConsoleColor tagColor = ConsoleColor.DarkGray)
        {
            // timestamp
            Write($"[{timeStamp}]", ConsoleColor.DarkGreen);

            // log type
            if (logType == LogType.INFO)
            {
                Write($":/> ", GetLogTypeColor(logType));
            }
            else
            {
                Write($":{LogTypeToString(logType)}/> ", GetLogTypeColor(logType));
            }

            // tag
            if (tag != null)
                Write($"#{tag}: ", tagColor);

            // message
            Write($"{msg}", msgColor);

            if (logType == LogType.ERROR || logType == LogType.WARNING)
            {
                Write("\r\n");
                Write($"CallStack=({string.Join("\r\n", CallStacks)})", ConsoleColor.DarkYellow);
            }

            // CRLF
            Write("\r\n");

            ResetColor();
        }

        public static void Write(string msg)
        {
            Console.Write(msg);
        }

        public static void Write(string msg, ConsoleColor cr)
        {
            ConsoleColor oldColor = SetColor(cr);
            Write(msg);
            SetColor(oldColor);
        }

        public static void WriteLine(string msg)
        {
            Console.WriteLine(msg);
        }

        public static void WriteLine(string msg, ConsoleColor cr)
        {
            ConsoleColor oldColor = SetColor(cr);
            WriteLine(msg);
            SetColor(oldColor);
        }

        private static ConsoleColor SetColor(ConsoleColor cr)
        {
            ConsoleColor oldColor = Console.ForegroundColor;
            Console.ForegroundColor = cr;
            return oldColor;
        }

        private static void ResetColor()
        {
            Console.ResetColor();
        }

        private static ConsoleColor GetLogTypeColor(LogType logType)
        {
            switch (logType)
            {
                case LogType.ERROR: return ConsoleColor.Red;
                case LogType.WARNING: return ConsoleColor.Yellow;
                case LogType.DEBUG: return ConsoleColor.Cyan;
                case LogType.VERB: return ConsoleColor.Magenta;
                case LogType.USER: return ConsoleColor.White;
                case LogType.INFO:
                default:
                    break;
            }

            return ConsoleColor.Gray;
        }

        public static void PressAnyKey(string msg = "Press any key to continue...")
        {
            i(msg);
            Console.ReadLine();
        }
    }
}
