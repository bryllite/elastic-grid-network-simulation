using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Bryllite.Util.Log
{
    public class BConsole
    {
        public static readonly string CRLF = "\r\n";

        public static string DateCode => DateTime.Now.ToString("yyyy-MM-dd");
        public static string TimeCode => DateTime.Now.ToString("HH:mm:ss.fff");

        public static ConsoleColor ColorTimeCode = ConsoleColor.DarkGreen;
        public static ConsoleColor ColorError = ConsoleColor.DarkRed;
        public static ConsoleColor ColorWarning = ConsoleColor.DarkYellow;
        public static ConsoleColor ColorDebug = ConsoleColor.DarkCyan;
        public static ConsoleColor ColorTrace = ConsoleColor.DarkGray;
        public static ConsoleColor ColorInfo = ConsoleColor.DarkMagenta;
        public static ConsoleColor ColorVerb = ConsoleColor.DarkBlue;
        public static string[] CallStacks
        {
            get
            {
                List<string> callstacks = new List<string>();

                foreach (var frame in new StackTrace().GetFrames())
                {
                    var frameMethod = frame.GetMethod();

                    string moduleName = frameMethod.Module.ToString();
                    string className = frameMethod.ReflectedType.Name;
                    string methodName = frameMethod.Name;

                    // exclude this
                    if (className != MethodBase.GetCurrentMethod().DeclaringType.Name)
                        callstacks.Add($"{moduleName}.{className}.{methodName}()");
                }

                return callstacks.ToArray();
            }
        }

        public static string Caller => CallStacks.Length > 0 ? CallStacks[0] : "" ;

        // console lock object
        public static object Lock = new object();

        public static ConsoleColor TextColor
        {
            get { return Console.ForegroundColor; }
            set { Console.ForegroundColor = value; }
        }

        public static ConsoleColor BackColor
        {
            get { return Console.BackgroundColor; }
            set { Console.BackgroundColor = value; }
        }

        public static void Clear()
        {
            Console.Clear();
        }

        public static void Beep()
        {
            Console.Beep();
        }

        public static void Beep(int frequency, int duration)
        {
            Console.Beep(frequency, duration);
        }

        public static void Write(object value)
        {
            Console.Write(value);
        }

        public static void Write(ConsoleColor textColor, object value)
        {
            lock (Lock)
            {
                SetColor(textColor);
                Write(value);
                ResetColor();
            }
        }

        public static void Write(ConsoleColor textColor, ConsoleColor backColor, object value)
        {
            lock (Lock)
            {
                SetColor(textColor, backColor);
                Write(value);
                ResetColor();
            }
        }

        public static void Write(string message)
        {
            Console.Write(message);
        }

        public static void Write(string message, params object[] args)
        {
            Write($"{message} {string.Join(", ", args)}");
        }

        public static void Write(ConsoleColor textColor, string message)
        {
            lock (Lock)
            {
                SetColor(textColor);
                Write(message);
                ResetColor();
            }
        }

        public static void Write(ConsoleColor textColor, string message, params object[] args)
        {
            lock (Lock)
            {
                SetColor(textColor);
                Write(message, args);
                ResetColor();
            }
        }

        public static void Write(ConsoleColor textColor, ConsoleColor backColor, string message)
        {
            lock (Lock)
            {
                SetColor(textColor, backColor);
                Write(message);
                ResetColor();
            }
        }

        public static void Write(ConsoleColor textColor, ConsoleColor backColor, string message, params object[] args)
        {
            lock (Lock)
            {
                SetColor(textColor, backColor);
                Write(message, args);
                ResetColor();
            }
        }

        public static void WriteLine()
        {
            Console.WriteLine();
        }

        public static void WriteLine(string message)
        {
            Console.WriteLine(message);
        }

        public static void WriteLine(string message, params object[] args)
        {
            WriteLine($"{message} {string.Join(", ", args)}");
        }

        public static void WriteLine(ConsoleColor textColor, string message)
        {
            lock (Lock)
            {
                SetColor(textColor);
                WriteLine(message);
                ResetColor();
            }
        }

        public static void WriteLine(ConsoleColor textColor, string message, params object[] args)
        {
            lock (Lock)
            {
                SetColor(textColor);
                WriteLine(message, args);
                ResetColor();
            }
        }

        public static void WriteLine(ConsoleColor textColor, ConsoleColor backColor, string message)
        {
            lock (Lock)
            {
                SetColor(textColor, backColor);
                WriteLine(message);
                ResetColor();
            }
        }

        public static void WriteLine(ConsoleColor textColor, ConsoleColor backColor, string message, params object[] args)
        {
            lock (Lock)
            {
                SetColor(textColor, backColor);
                WriteLine(message, args);
                ResetColor();
            }
        }

        public static void SetColor(ConsoleColor textColor, ConsoleColor backColor)
        {
            TextColor = textColor;
            BackColor = backColor;
        }

        public static void SetColor(ConsoleColor textColor)
        {
            TextColor = textColor;
        }

        public static ConsoleColor SetTextColor(ConsoleColor textColor)
        {
            ConsoleColor oldColor = TextColor;
            TextColor = textColor;
            return oldColor;
        }

        public static ConsoleColor SetBackColor(ConsoleColor backColor)
        {
            ConsoleColor oldColor = BackColor;
            BackColor = backColor;
            return oldColor;
        }

        public static void ResetColor()
        {
            Console.ResetColor();
        }

        public static void WriteF(string format, params object[] args)
        {
            Console.Write(format, args);
        }

        public static void WriteF(ConsoleColor textColor, string format, params object[] args)
        {
            lock (Lock)
            {
                SetColor(textColor);
                WriteF(format, args);
                ResetColor();
            }
        }

        public static void WriteF(ConsoleColor textColor, ConsoleColor backColor, string format, params object[] args)
        {
            lock (Lock)
            {
                SetColor(textColor, backColor);
                WriteF(format, args);
                ResetColor();
            }
        }

        public static void WriteLineF(string format, params object[] args)
        {
            Console.WriteLine(format, args);
        }

        public static void WriteLineF(ConsoleColor textColor, string format, params object[] args)
        {
            lock (Lock)
            {
                SetColor(textColor);
                WriteLineF(format, args);
                ResetColor();
            }
        }

        public static void WriteLineF(ConsoleColor textColor, ConsoleColor backColor, string format, params object[] args)
        {
            lock (Lock)
            {
                SetColor(textColor, backColor);
                WriteLineF(format, args);
                ResetColor();
            }
        }

        public static void WriteCallStacks()
        {
            WriteCallStacks(CallStacks.Length);
        }

        public static void WriteCallStacks(int frame)
        {
            WriteLine(ConsoleColor.DarkGray, $"StackTrace: '{string.Join("\r\n  at ", CallStacks.Take(frame).ToArray())}'");
        }


        public static void Error(string message)
        {
            lock (Lock)
            {
                Write(ColorTimeCode, $"[{TimeCode}] ");
                Write(ColorError, "ERROR ");
                WriteLine(message);
                WriteCallStacks();
            }
        }

        public static void Error(string message, params object[] args)
        {
            Error($"{message} {string.Join(" ", args)}");
        }

        public static void Warning(string message)
        {
            lock (Lock)
            {
                Write(ColorTimeCode, $"[{TimeCode}] ");
                Write(ColorWarning, "WARNING ");
                WriteLine(message);
            }
        }

        public static void Warning(string message, params object[] args)
        {
            Warning($"{message} {string.Join(" ", args)}");
        }

        public static void Debug(string message)
        {
            lock (Lock)
            {
                Write(ColorTimeCode, $"[{TimeCode}] ");
                Write(ColorDebug, "DEBUG ");
                WriteLine(message);
            }
        }

        public static void Debug(string message, params object[] args)
        {
            Debug($"{message} {string.Join(" ", args)}");
        }

        public static void Trace(string message)
        {
            lock (Lock)
            {
                Write(ColorTimeCode, $"[{TimeCode}] ");
                Write(ColorTrace, "TRACE ");
                WriteLine(message);
            }
        }

        public static void Trace(string message, params object[] args)
        {
            Trace($"{message} {string.Join(" ", args)}");
        }


        public static void Info(string message)
        {
            lock (Lock)
            {
                Write(ColorTimeCode, $"[{TimeCode}] ");
                Write(ColorInfo, "INFO ");
                WriteLine(message);
            }
        }

        public static void Info(string message, params object[] args)
        {
            Info($"{message} {string.Join(" ", args)}");
        }

        public static void Verb(string message)
        {
            lock (Lock)
            {
                Write(ColorTimeCode, $"[{TimeCode}] ");
                Write(ColorVerb, "VERB ");
                WriteLine(message);
            }
        }

        public static void Verb(string message, params object[] args)
        {
            Verb($"{message} {string.Join(" ", args)}");
        }


        public class MessageBuilder
        {
            private static readonly ConsoleColor DefaultTextColor = ConsoleColor.Gray;
            private static readonly ConsoleColor DefaultBackColor = ConsoleColor.Black;

            // message list with color
            private List<(ConsoleColor textColor, ConsoleColor backColor, string message)> _messages = new List<(ConsoleColor textColor, ConsoleColor backColor, string message)>();

            public MessageBuilder Append(ConsoleColor textColor, ConsoleColor backColor, object message)
            {
                _messages.Add((textColor, backColor, message.ToString()));
                return this;
            }

            public MessageBuilder Append(ConsoleColor textColor, object message)
            {
                return Append(textColor, DefaultBackColor, message);
            }

            public MessageBuilder Append(object message)
            {
                return Append(DefaultTextColor, DefaultBackColor, message);
            }

            public MessageBuilder AppendLine(ConsoleColor textColor, ConsoleColor backColor, object message)
            {
                _messages.Add((textColor, backColor, message.ToString() + CRLF));
                return this;
            }

            public MessageBuilder AppendLine(ConsoleColor textColor, object message)
            {
                return AppendLine(textColor, DefaultBackColor, message);
            }

            public MessageBuilder AppendLine(object message)
            {
                return AppendLine(DefaultTextColor, DefaultBackColor, message);
            }


            public void Write()
            {
                Write(TextColor, BackColor, "");
            }

            public void Write(object message)
            {
                Write(TextColor, DefaultBackColor, message);
            }

            public void Write(ConsoleColor textColor, object message)
            {
                Write(textColor, DefaultBackColor, message);
            }

            public void Write( ConsoleColor textColor, ConsoleColor backColor, object message )
            {
                lock (Lock)
                {
                    foreach (var m in _messages)
                    {
                        SetColor(m.textColor, m.backColor);
                        BConsole.Write(m.message);
                    }

                    BConsole.Write(textColor, backColor, message.ToString());
                }
            }

            public void WriteLine()
            {
                WriteLine(DefaultTextColor, DefaultBackColor, "");
            }

            public void WriteLine(object message)
            {
                WriteLine(DefaultTextColor, DefaultBackColor, message);
            }

            public void WriteLine(ConsoleColor textColor, object message)
            {
                WriteLine(textColor, DefaultBackColor, message);
            }

            public void WriteLine( ConsoleColor textColor, ConsoleColor backColor, object message )
            {
                lock (Lock)
                {
                    foreach (var m in _messages)
                    {
                        SetColor(m.textColor, m.backColor);
                        BConsole.Write(m.message);
                    }

                    BConsole.WriteLine( textColor, backColor, message.ToString() );
                }
            }
        }

    }
}
