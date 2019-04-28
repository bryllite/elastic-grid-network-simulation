using System;
using System.Threading;
using Bryllite.Util.Log;

namespace Bryllite.Net.Tcp.Server.Tests
{
    class Program
    {
        public static readonly int PORT = 30303;

        static void Main(string[] args)
        {
            ILoggable logger = new BLog.Builder()
                .WithConsole(true)
                .WithFilter(LogFilter.All)
                .Build();

            logger.info("Hello, Bryllite!");

            using (CancellationTokenSource cts = new CancellationTokenSource())
            {
                Console.CancelKeyPress += (sender, e) =>
                {
                    e.Cancel = true;
                    cts.Cancel();
                };

                TestTcpServer server = new TestTcpServer(logger);
                server.Start(TcpHelper.ANY, PORT);

                while (!cts.IsCancellationRequested)
                {
                    Thread.Sleep(10);
                }

                server.Stop();
            }

            logger.info("Bye, Bryllite!");
        }
    }
}
