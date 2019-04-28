using System;
using System.IO;
using System.Threading;
using System.Timers;
using Bryllite.Core.Key;
using Bryllite.Net.Elastic;
using Bryllite.Util.Log;
using Microsoft.Extensions.Configuration;

namespace Bryllite.App.PeerListServiceApp
{
    class Program
    {
        // config file name
        public static readonly string APPSETTINGS = "appsettings.json";

        static void Main(string[] args)
        {
            // load Configuration
            IConfiguration Configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile(APPSETTINGS, optional: true, reloadOnChange: true)
                .Build();

            // logger
            ILoggable Logger = new BLog.Builder()
                .WithConsole(true)
                .WithLogger(new RotateFileLogger(Configuration.GetValue("Logging:LogFileName", "PeerListServiceApp.log")))
                .Global(true)
                .WithFilter(LogFilter.All)
                .Build();

            Logger.info($"Hello, Bryllite!");

            // node map provider
            IPeerList peers = new PeerList(Logger);

            using (var cts = new CancellationTokenSource())
            {
                PeerListService server = new PeerListService(Configuration.GetSection("PeerListService"), Logger, peers);
                server.Start(32, cts);

                // garbage collection
                //System.Timers.Timer gcTimer = new System.Timers.Timer(30 * 1000);
                //gcTimer.Elapsed += OnGarbageCollect;
                //gcTimer.Enabled = true;

                Console.CancelKeyPress += (sender, e) =>
                {
                    e.Cancel = true;
                    cts.Cancel();
                };

                while (!cts.Token.IsCancellationRequested)
                {
                    Thread.Sleep(10);

                    server.Update();
                }

                server.Stop();
            }

            Logger.info($"Bye, Bryllite!");
        }

        static void OnGarbageCollect(object sender, ElapsedEventArgs e)
        {
            GC.Collect(0, GCCollectionMode.Forced);
            GC.WaitForFullGCComplete();
        }
    }
}
