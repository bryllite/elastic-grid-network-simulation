using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Bryllite.Core.Key;
using Bryllite.Net.Elastic;
using Bryllite.Net.Messages;
using Bryllite.Net.Tcp;
using Bryllite.Util;
using Bryllite.Util.Log;

namespace Bryllite.MemoryLeaks.Tests
{
    class Program
    {
        static readonly int PORT = 19999;

        static void Main(string[] args)
        {
            ILoggable logger = new BLog.Builder()
                .WithConsole(true)
                .WithFilter(LogFilter.All)
                .Build();

            PrivateKey key = PrivateKey.CreateKey();

            using (CancellationTokenSource cts = new CancellationTokenSource())
            {
                NodeServer server = new NodeServer(logger);
                server.Start(PORT, cts );

                Console.CancelKeyPress += (sender, e) =>
                {
                    e.Cancel = true;
                    cts.Cancel();
                };

                byte[] rndBytes = RndProvider.GetNonZeroBytes(1024 * 1024);

                while (!cts.IsCancellationRequested)
                {
                    Thread.Sleep(10);

                    rndBytes = RndProvider.GetNonZeroBytes(1024 * 1024);
                    Message message = new Message.Builder()
                        .Body("rndBytes", rndBytes)
                        .Build(key);

                    byte[] data = message.ToBytes();
                    //                    ElasticNode.SendTo(data, "127.0.0.1", PORT);
                    NodeClient client = new NodeClient(logger);
                    client.Start("127.0.0.1", PORT);

                    Thread.Sleep(1000);

//                    client.Write(data);
                    client.Stop();
                }
            }

        }
    }
}
