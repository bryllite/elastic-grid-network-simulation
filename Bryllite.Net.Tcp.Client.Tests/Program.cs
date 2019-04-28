using System;
using System.Threading;
using Bryllite.Util;
using Bryllite.Util.Log;
using Bryllite.Util.Payloads;

namespace Bryllite.Net.Tcp.Client.Tests
{
    class Program
    {
        public static readonly int PORT = 30303;
        public static readonly int MessageLength = 1024 * 1024;

        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            ILoggable logger = new BLog.Builder()
                .WithConsole(true)
                .WithFilter(LogFilter.All)
                .Build();

            using (CancellationTokenSource cts = new CancellationTokenSource())
            {
                Console.CancelKeyPress += (sender, e) =>
                {
                    e.Cancel = true;
                    cts.Cancel();
                };

                while (!cts.IsCancellationRequested)
                {
//                    Thread.Sleep(100);

                    Payload payload = new Payload.Builder()
                        .Value("rndBytes", RndProvider.GetNonZeroBytes(MessageLength))
                        .Build();

                    TcpClient.SendTo("127.0.0.1", PORT, payload.ToBytes());
                }
            }

        }
    }
}
