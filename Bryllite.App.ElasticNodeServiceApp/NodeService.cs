using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Bryllite.Core.Key;
using Bryllite.Net.Elastic;
using Bryllite.Net.Messages;
using Bryllite.Net.Tcp;
using Bryllite.Util;
using Bryllite.Util.CommandLine;
using Bryllite.Util.Log;
using Bryllite.Util.Payloads;
using Microsoft.Extensions.Configuration;

namespace Bryllite.App.ElasticNodeServiceApp
{
    public class NodeService
    {
        // configuration
        private IConfiguration Configuration;

        // logger
        private ILoggable Logger;

        // Elastic Node host
        private IElasticNode ElasticNode;

        // node list provider
        private IPeerList Peers;

        // Node service endpoint
        public ElasticAddress EndPoint;

        // Node service master key
        public PrivateKey NodeKey;

        // catalog service
        private PeerListClient PeerListClient;

        // console command processor
        private CommandLineInterpreter commandLineInterpreter;

        // elastic grid network test 
        private ElasticGridNetworkTests Test;

        private Payload Env => Test?.Env;

        public NodeService(IConfiguration configuration, ILoggable logger, IPeerList peers )
        {
            Configuration = configuration;
            Logger = logger;
            Peers = peers;

            // create catalog service
            PeerListClient = new PeerListClient(configuration.GetSection("PeerListClient"), logger, peers);
        }

        private CancellationTokenSource cts;

        public void Start(int port, int acceptWorkers, CancellationTokenSource cts)
        {
            this.cts = cts;

            string host = Configuration.GetValue("Host", TcpHelper.ANY);
            if (port <= 0)
                port = Configuration.GetValue("Port", 9000);

            NodeKey = PeerListClient.MasterKey.CKD($"{host}:{port}");
            EndPoint = new ElasticAddress(NodeKey.Address, host, port);

            // start catalog service
            PeerListClient.Start( NodeKey, EndPoint );

            // Elastic Node Host start
            ElasticNode = new ElasticNode(Configuration, Logger, Peers, NodeKey, EndPoint)
            {
                OnMessage = OnElasticMessage
            };
            ElasticNode.Start( cts );

            // ElasticGridNetworkTests
            Test = new ElasticGridNetworkTests(Logger, ElasticNode, Peers, NodeKey);
          
        }

        public void Stop()
        {
            // close all forked process
            Test?.KillAll();

            // stop catalog service
            PeerListClient.Stop();

            // stop elastic node
            ElasticNode.Stop();
        }


        public void StartConsole()
        {
            commandLineInterpreter = new CommandLineInterpreter()
            {
                OnCommand = OnConsoleCommand
            };

            ShowEnviromentValues();
            ShowUsage();

            BConsole.Enabled = true;
            commandLineInterpreter.Start( cts );
        }

        public void StopConsole()
        {
            commandLineInterpreter.Stop();
        }

        public void OnConsoleCommand(string command, string[] args)
        {
            switch (command.ToLower())
            {
                case "quit": case "shutdown": case "exit": OnConsoleCommandExit(args); break;
                case "h": case "help": ShowUsage(); break;
                case "set": OnConsoleCommandSet(args); break;
                case "env": ShowEnviromentValues(); break;
                case "cls": ClearScreen(); break;
                case "test.start": OnConsoleCommandTestStart(args); break;
                case "test.prepare": OnConsoleCommandTestPrepare(args); break;
                case "test.run": OnConsoleCommandTestRun(args); break;
                case "test.stop": OnConsoleCommandTestStop(args); break;
                case "test.report": OnConsoleCommandTestReport(args); break;
                case "test.clear": OnConsoleCommandTestClear(args); break;
                case "peers": OnConsoleCommandPeersList(args); break;
                case "peers.sync": OnConsoleCommandPeersSync(args); break;
                case "peers.layout": OnConsoleCommandPeersLayout(args); break;
                case "peers.kill": OnConsoleCommandPeersKill(args); break;
                default: Logger.warning("unknown command!\r\n: type 'h' for command list."); break;
            }
        }

        private void ShowUsage()
        {
            BConsole.WriteLine();
            BConsole.WriteLine("COMMANDS:");
            BConsole.WriteLineF("  {0,-25} {1}", "quit, exit, shutdown", "shutdown all process and exit");
            BConsole.WriteLineF("  {0,-25} {1}", "h, help", "show usage message");
            BConsole.WriteLineF("  {0,-25} {1}", "env", "show current environment values");
            BConsole.WriteLineF("  {0,-25} {1}", "set(name, value)", "set environment value (ex: `set(n,16)`)");
            BConsole.WriteLineF("  {0,-25} {1}", "cls", "clear screen");

            BConsole.WriteLineF("  {0,-25} {1}", "test.start()", "start test with current environment values");
            BConsole.WriteLineF("  {0,-25} {1}", "test.prepare(nPeers)", "prepare [nPeers] processes fork");
            BConsole.WriteLineF("  {0,-25} {1}", "test.run()", "run test with current environment values and processes");
            BConsole.WriteLineF("  {0,-25} {1}", "test.stop()", "stop test");
            BConsole.WriteLineF("  {0,-25} {1}", "test.report()", "show test reports");

            BConsole.WriteLineF("  {0,-25} {1}", "peers", "show peer list information");
            BConsole.WriteLineF("  {0,-25} {1}", "peers.layout(n)", "show elastic grid layout for current peers");
            BConsole.WriteLineF("  {0,-25} {1}", "peers.kill(start, end)", "kill forked process from [start] port to [end] port");
            BConsole.WriteLineF("  {0,-25} {1}", "peers.sync()", "request PeerListService to peer sync");
            BConsole.WriteLine();
        }

        private void ShowEnviromentValues()
        {
            BConsole.WriteLine();
            BConsole.WriteLine("ENVIRONMENT VALUES:");
            BConsole.WriteLine(ConsoleColor.Cyan, Env.ToString());
            BConsole.WriteLine();
        }

        private void OnConsoleCommandExit(string[] args)
        {
            cts?.Cancel();
        }

        private void OnConsoleCommandSet(string[] args)
        {
            try
            {
                string name = args[0];
                string value = args[1];

                Test.Env.Set(name, value);
            }
            catch (Exception e)
            {
                BConsole.Error($"argument exception! e={e.Message}");
            }

            ShowEnviromentValues();
        }

        private void ClearScreen()
        {
            BConsole.Clear();
        }

        private void OnConsoleCommandTestStart(string[] args)
        {
            // prepare processes
            Test?.Prepare(EndPoint.Host, EndPoint.Port);

            // wait for peer list sync
            PeerListClient.RequestSync();

            BConsole.Info("please wait for the peer list to sync...");
            Thread.Sleep(Peers.Count * 128);

            // start message test
            Test?.Start();
        }


        private void OnConsoleCommandTestPrepare(string[] args)
        {
            if (args.Length > 0)
            {
                try
                {
                    int nPeers = Convert.ToInt32(args[0]);
                    Env?.Set("nPeers", nPeers);
                }
                catch (Exception e)
                {
                    BConsole.Error($"argument exception! e={e.Message}");
                    return;
                }
            }

            // prepare processes
            Test?.Prepare(EndPoint.Host, EndPoint.Port);

            // wait for peer list sync
            PeerListClient.RequestSync();
        }

        private void OnConsoleCommandTestRun(string[] args)
        {
            if (Peers.Count > 0)
            {
                // 네트워크 테스트 시작
                Test?.Start();
            }
            else
            {
                BConsole.Error("PeerList not synchronized");
            }
        }

        private void OnConsoleCommandTestStop(string[] args)
        {
            Test?.Stop();
        }

        private void OnConsoleCommandTestReport(string[] args)
        {
            Test?.Report();
        }

        private void OnConsoleCommandTestClear(string[] args)
        {
            Test?.ClearReport();
        }

        private void OnConsoleCommandPeersList(string[] args)
        {
            foreach (var peer in Peers.Peers)
                BConsole.WriteLine(peer);

            BConsole.WriteLine(ConsoleColor.DarkCyan, $"nPeers={Peers.Count}");
        }

        private void OnConsoleCommandPeersSync(string[] args)
        {
            PeerListClient.RequestSync();
        }

        private void OnConsoleCommandPeersLayout(string[] args)
        {
            byte n = Env.Value<byte>("n", 4);

            if (args.Length > 0)
                byte.TryParse(args[0], out n);

            // define layout
            ElasticLayout layout = ElasticLayout.DefineLayout(Peers.Count, n);

            for (byte x = 1; x <= layout.X; x++)
            {
                for (byte y = 1; y <= layout.Y; y++)
                {
                    for (byte z = 1; z <= layout.Z; z++)
                    {
                        Elastic3D coordinates = new Elastic3D(x, y, z);
                        string[] peers = Peers.ToArray<string>(coordinates, layout);

                        BConsole.WriteLine(ConsoleColor.DarkCyan, $"Coordinates{coordinates.ToString()}: nPeers={peers.Length}");
                        foreach (var peer in peers)
                            BConsole.WriteLine(EndPoint.ToString()==peer?ConsoleColor.Yellow : ConsoleColor.Gray, $" {peer}");
                    }
                }
            }

            BConsole.WriteLine(ConsoleColor.DarkCyan, $"ElasticLayout.DefineLayout({Peers.Count},{n})={layout.ToString()}");
        }

        private void OnConsoleCommandPeersKill(string[] args)
        {
            int portStart = Convert.ToInt32(args[0]);
            int portEnd = args.Length >= 2 ? Convert.ToInt32(args[1]) : portStart;

            for (int i = portStart; i <= portEnd; i++)
                Test?.Kill(i);
        }

        public void Update()
        {
        }

        public void OnElasticMessage(ElasticAddress sender, Message message)
        {
            switch (message.Action())
            {
                case "ping": OnTcpMessagePing(sender, message); break;
                case "pong": OnTcpMessagePong(sender, message); break;
                default: break;
            }
        }

        private int OnTcpMessagePing(ElasticAddress sender, Message message)
        {
            Test?.OnMessagePing(sender, message);
            return 0;
        }

        private int OnTcpMessagePong(ElasticAddress sender, Message message)
        {
            Test?.OnMessagePong(sender, message);
            return 0;
        }
    }
}
