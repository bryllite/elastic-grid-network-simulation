using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Bryllite.Util.Log
{
    public class BLog : ILoggable
    {
        // global logger
        public static ILoggable Global;

        // console & debugger logger instance
        public static ILoggable Console = ConsoleLogger.Instance;
        public static ILoggable Debug = DebugLogger.Instance;

        // logger chain
        private List<ILoggable> _loggers;

        // log filter ( default: all )
        public LogLevel mLogFilter { get; set; } = LogFilter.None;

        private BLog( bool global )
        {
            _loggers = new List<ILoggable>();

            if (global) Global = this;
        }

        public void Attach(ILoggable logger)
        {
            lock (_loggers)
            {
                _loggers.Add(logger);
            }
        }

        public void Attach(ILoggable logger, LogLevel filter)
        {
            Attach(logger);
            logger.mLogFilter = filter;
        }


        public void Dettach(ILoggable logger)
        {
            lock (_loggers)
            {
                _loggers.Remove(logger);
            }
        }

        public ILoggable[] Loggers
        {
            get
            {
                lock (_loggers) { return _loggers.ToArray(); }
            }
        }

        public void LogWrite(LogLevel logLevel, string tag, string message)
        {
            if (string.IsNullOrEmpty(tag))
            {
                LogWrite(logLevel, message);
            }
            else
            {
                if (!mLogFilter.Match(logLevel)) return;

                foreach (var logger in Loggers)
                    logger.LogWrite(logLevel, tag, message);
            }
        }

        public void LogWrite(LogLevel logLevel, string message)
        {
            if (!mLogFilter.Match(logLevel)) return;

            foreach (var logger in Loggers)
                logger.LogWrite(logLevel, message);
        }

        public void LogWrite(string message)
        {
            foreach (var logger in Loggers)
                logger.LogWrite(message);
        }

        public class Builder
        {
            private List<ILoggable> _loggers = new List<ILoggable>();
            private bool _global = false;
            private bool _console = false;
            private LogLevel _logFilter = LogFilter.None;

            public Builder WithLogger(ILoggable logger)
            {
                _loggers.Add(logger);
                return this;
            }

            public Builder WithConsole(bool console)
            {
                _console = console;
                return this;
            }

            public Builder WithFilter(LogLevel logFilter)
            {
                _logFilter = logFilter;
                return this;
            }

            public Builder Global(bool global)
            {
                _global = global;
                return this;
            }

            public BLog Build()
            {
                // allocate new BLog
                BLog bLogger = new BLog(_global);

                bool filter = _logFilter != LogFilter.None;
                if (filter)
                    bLogger.mLogFilter = _logFilter;

                // enable debug logger
                if (Debugger.IsAttached)
                {
                    if (filter) bLogger.Attach(Debug, _logFilter);
                    else bLogger.Attach(Debug);
                }

                // enable console logger
                if (_console)
                {
                    if (filter) bLogger.Attach(Console, _logFilter);
                    else bLogger.Attach(Console);
                }

                // attach each logger
                foreach (var logger in _loggers)
                {
                    if (filter) bLogger.Attach(logger, _logFilter);
                    else bLogger.Attach(logger);
                }

                return bLogger;
            }
        }
    }
}
