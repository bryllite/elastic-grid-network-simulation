using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using Bryllite.Core.Hash;
using Bryllite.Core.Key;
using Bryllite.Net.Elastic;
using Bryllite.Util;
using Bryllite.Util.CommandLine;
using Bryllite.Util.Log;
using Bryllite.Util.Payloads;
using Microsoft.Extensions.Configuration;

namespace Bryllite.App.ElasticNodeServiceApp
{
    public class Program
    {
        // config file name
        public static readonly string APPSETTINGS = "appsettings.json";

        // configuration
        public static IConfiguration Configuration;

        // logger
        public static ILoggable Logger;

        // command line parser
        public static CommandLineParser commandLineParser ;

        // node service
        public static NodeService NodeService;


        static void Main(string[] args)
        {
            // command line parser
            commandLineParser = new CommandLineParser(args);

            // load Configuration
            Configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile(APPSETTINGS, optional: true, reloadOnChange: true)
                .Build();

            // logger
            Logger = new BLog.Builder()
                .WithConsole(true)
//                .WithLogger(new RotateFileLogger($"logs/{commandLineParser.Value("host", "localhost")}-{commandLineParser.Value("port", 0)}.log"))
                .Global(true)
                .WithFilter(LogFilter.All)
                .Build();

            Logger.info($"Hello, Bryllite! args={commandLineParser.ToString()}");

            // peer list provider
            IPeerList peers = new PeerList(Logger);

            using (var cts = new CancellationTokenSource())
            {
                NodeService = new NodeService(Configuration.GetSection("NodeService"), Logger, peers );
                NodeService.Start(commandLineParser.Value("port", 0), 16, cts);

                Console.CancelKeyPress += (sender, e) =>
                {
                    e.Cancel = true;
                    cts.Cancel();
                };

                // console command
                if (commandLineParser.Value("console", false))
                    NodeService.StartConsole();

                while (!cts.Token.IsCancellationRequested)
                {
                    Thread.Sleep(10);

                    NodeService.Update();
                }

                NodeService.Stop();
            }

            Logger.info($"Bye, Bryllite!");
        }
    }
}
