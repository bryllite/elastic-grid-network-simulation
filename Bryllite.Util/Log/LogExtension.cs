using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Bryllite.Util.Log
{
    public static class LogExtension
    {
        public static readonly string CRLF = "\r\n";

        public static string DateCode
        {
            get
            {
                return DateTime.Now.ToString("yyyy-MM-dd");
            }
        }

        public static string TimeCode
        {
            get
            {
                return DateTime.Now.ToString("HH:mm:ss.fff");
            }
        }

        public static string[] CallStacks
        {
            get
            {
                List<string> callStacks = new List<string>();

                var stackTrace = new StackTrace();
                var stackFrames = stackTrace.GetFrames();

                foreach (var frame in stackFrames)
                {
                    var FrameMethod = frame.GetMethod();

                    string className = FrameMethod.ReflectedType?.Name;
                    string method = FrameMethod.Name;

                    callStacks.Add($"{className}::{method}()");
                }

                return callStacks.ToArray();
            }
        }

        public static void error(this ILoggable logger, params string[] messages)
        {
            logger?.LogWrite(LogLevel.ERROR, string.Join(", ", messages));
        }

        public static void warning(this ILoggable logger, params string[] messages)
        {
            logger?.LogWrite(LogLevel.WARNING, string.Join(", ", messages));
        }

        public static void debug(this ILoggable logger, params string[] messages)
        {
            logger?.LogWrite(LogLevel.DEBUG, string.Join(", ", messages));
        }

        public static void trace(this ILoggable logger, params string[] messages)
        {
            logger?.LogWrite(LogLevel.TRACE, string.Join(", ", messages));
        }

        public static void info(this ILoggable logger, params string[] messages)
        {
            logger?.LogWrite(LogLevel.INFO, string.Join(", ", messages));
        }

        public static void verb(this ILoggable logger, params string[] messages)
        {
            logger?.LogWrite(LogLevel.VERB, string.Join(", ", messages));
        }

        public static void native(this ILoggable logger, string message, params object[] objs)
        {
            logger?.LogWrite($"{message} {string.Join(", ", objs)}");
        }

        //public static void error(this ILoggable logger, string tag, string message)
        //{
        //    logger?.LogWrite(LogLevel.ERROR, tag, message);
        //}

        //public static void warning(this ILoggable logger, string tag, string message)
        //{
        //    logger?.LogWrite(LogLevel.WARNING, tag, message);
        //}

        //public static void debug(this ILoggable logger, string tag, string message)
        //{
        //    logger?.LogWrite(LogLevel.DEBUG, tag, message);
        //}

        //public static void trace(this ILoggable logger, string tag, string message)
        //{
        //    logger?.LogWrite(LogLevel.TRACE, tag, message);
        //}

        //public static void info(this ILoggable logger, string tag, string message)
        //{
        //    logger?.LogWrite(LogLevel.INFO, tag, message);
        //}

        //public static void verb(this ILoggable logger, string tag, string message)
        //{
        //    logger?.LogWrite(LogLevel.VERB, tag, message);
        //}
    }
}
