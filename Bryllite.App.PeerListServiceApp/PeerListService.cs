using System;
using System.Collections.Generic;
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

namespace Bryllite.App.PeerListServiceApp
{
    public class PeerListService
    {
        public const string ACTION = "action";
        public const string ACTION_CLEAR = "clear";
        public const string ACTION_REGISTER = "register";
        public const string ACTION_UNREGISTER = "unregister";
        public const string ACTION_PEEK = "peek";
        public const string ACTION_UPDATE = "update";
        public const string ACTION_SYNC = "sync";

        // configuration
        private IConfiguration Configuration;

        // logger
        private ILoggable Logger;

        // NodeMap Provider
        private IPeerList Peers;

        // tcp server
        private ITcpServer Server;

        // catalog service endpoint
        private ElasticAddress _EndPoint = null;
        public ElasticAddress EndPoint
        {
            get
            {
                if (ReferenceEquals(_EndPoint, null)) _EndPoint = new ElasticAddress(Configuration.GetValue<string>("Address"));
                return _EndPoint;
            }
        }

        // catalog service master key
        private PrivateKey _MasterKey = null;
        public PrivateKey MasterKey
        {
            get
            {
                if (ReferenceEquals(_MasterKey, null)) _MasterKey = new PrivateKey(Configuration.GetValue<string>("MasterKey"));
                return _MasterKey;
            }
        }

        // session <-> ElasticAddress dictionary
        private Dictionary<ulong, Address> _sessionMap = new Dictionary<ulong, Address>();

        // console command processor
        private CommandLineInterpreter commandLineInterpreter;

        private CancellationTokenSource cts;

        // 환경 변수
        private Payload Env = new Payload.Builder()
            .Value("removeIfDisconnect", true )
            .Build();

        public PeerListService(IConfiguration configuration, ILoggable logger, IPeerList peers)
        {
            Configuration = configuration;
            Logger = logger;
            Peers = peers;

            if (MasterKey.Address != EndPoint.Address)
                throw new ArgumentException("MasterKey enode mismatch");

            // TCP server start
            Server = new TcpServer()
            {
                OnStart = OnTcpStart,
                OnStop = OnTcpStop,
                OnAccept = OnTcpAccept,
                OnClose = OnTcpClose,
                OnMessage = OnTcpMessage
            };

            commandLineInterpreter = new CommandLineInterpreter()
            {
                OnCommand = OnConsoleCommand
            };
        }

        public void Start(int acceptWorkers, CancellationTokenSource cts)
        {
            this.cts = cts;

            // catalog provider
            // do not load for test
            //CatalogProvider.Load();

            // start tcp server
            Server.Start(EndPoint.Host, EndPoint.Port, acceptWorkers);

            ShowEnv();
            ShowUsage();

            // start command line interpreter 
            commandLineInterpreter?.Start(cts);

        }

        public void Stop()
        {
            // stop tcp server
            Server.Stop();

            // stop command line interpreter
            commandLineInterpreter?.Stop();
        }

        public void Update()
        {
        }

        private Address FindAddress(ulong sid)
        {
            lock (_sessionMap)
            {
                return _sessionMap.ContainsKey(sid) ? _sessionMap[sid] : null;
            }
        }


        public void OnConsoleCommand(string command, string[] args)
        {
            switch (command.ToLower())
            {
                case "quit": case "shutdown": case "exit": OnConsoleCommandExit(args); break;
                case "h":  case "help": ShowUsage(); break;
                case "set": OnConsoleCommandSet(args); break;
                case "env": ShowEnv(); break;
                case "cls": ClearScreen(); break;
                case "peers.sync": OnConsoleCommandPeersSync(args); break;
                case "peers.layout": OnConsoleCommandPeersLayout(args); break;
                case "peers": OnConsoleCommandPeers(args); break;
                default: Logger.warning($"unknown command!\r\n: type 'h' for command list"); break;
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

            BConsole.WriteLineF("  {0,-25} {1}", "peers", "show peer list information");
            BConsole.WriteLineF("  {0,-25} {1}", "peers.layout(n)", "show elastic grid layout for current peers");
            BConsole.WriteLineF("  {0,-25} {1}", "peers.sync()", "request PeerListService to peer sync");
            BConsole.WriteLine();
        }

        private void ClearScreen()
        {
            BConsole.Clear();
        }

        public void OnConsoleCommandExit(string[] args)
        {
            Stop();
        }

        public void OnConsoleCommandSet(string[] args)
        {
            try
            {
                string name = args[0];
                string value = args[1];

                Env.Set(name, value);
            }
            catch (Exception e)
            {
                BConsole.Error($"argument exception! e={e.Message}");
            }

            ShowEnv();
        }

        private void ShowEnv()
        {
            BConsole.WriteLine();
            BConsole.WriteLine("ENVIRONMENT VALUES:");
            BConsole.WriteLine(ConsoleColor.Cyan, Env.ToString());
            BConsole.WriteLine();
        }

        public void OnConsoleCommandPeers(string[] args)
        {
            foreach (var peer in Peers.Peers)
                BConsole.WriteLine(peer);

            BConsole.WriteLine( ConsoleColor.DarkCyan, $"nPeers={Peers.Count}");
        }

        public void OnConsoleCommandPeersLayout(string[] args)
        {
            if (args.Length == 0)
            {
                BConsole.Warning("n value require! usage: peers.layout(16)");
                return;
            }

            byte n = 0;
            try
            {
                n = Convert.ToByte(args[0]);
            }
            catch (Exception e)
            {
                BConsole.Error($"convert n exception! e={e.Message}");
                return;
            }

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
                            BConsole.WriteLine($" {peer}");
                    }
                }
            }

            BConsole.WriteLine(ConsoleColor.DarkCyan, $"ElasticLayout.DefineLayout({Peers.Count},{n})={layout.ToString()}");
        }


        public void OnConsoleCommandPeersSync(string[] args)
        {
            SyncAllPeers(Peers.Peers);
        }

        public void OnTcpStart(string host, int port)
        {
            Logger.info($"PeerListService started {host}:{port} {EndPoint.ToString()}");
        }

        public void OnTcpStop()
        {
            Logger.info($"PeerListService stop");
        }

        public void OnTcpAccept(ITcpSession session)
        {
            Logger.info($"New connection established! sid={session.SID}, remote={session.Remote}");
        }

        public void OnTcpClose(ITcpSession session, int reason)
        {
            Logger.info($"Connection lost! sid={session.SID}, remote={session.Remote}");

            Address address = FindAddress(session.SID);
            if (null != address)
            {
                _sessionMap.Remove(session.SID);

                if (Env.Value("removeIfDisconnect", true))
                {
                    Peers.Remove(address);
                    SyncAllPeers();
                }
            }
        }

        public void OnTcpMessage(ITcpSession session, byte[] data)
        {
            Message message = Message.Parse(data);
            switch (message.Value<string>(ACTION))
            {
                case ACTION_CLEAR: OnTcpMessageClear(session, message); return;
                case ACTION_REGISTER: OnTcpMessageRegister(session, message); return;
                case ACTION_UNREGISTER: OnTcpMessageUnregister(session, message); return;
                case ACTION_PEEK: OnTcpMessagePeek(session, message); return;
                case ACTION_UPDATE: OnTcpMessageUpdate(session, message); return;
                case ACTION_SYNC: OnTcpMessageSync(session, message); return;
                default: break;
            }

            Logger.error($"unknown action! '{message.Value<string>(ACTION)}'");
        }

        private void OnTcpMessageClear(ITcpSession session, Message message)
        {
            if (!message.Verify(MasterKey.Address))
            {
                Logger.warning("Unverified message!");
                return ;
            }

            Peers.Clear();
        }

        private void OnTcpMessageRegister(ITcpSession session, Message message)
        {
            // register requested enode
            ElasticAddress address = message.Value<string>("enode");

            // verify message
            PrivateKey nodeKey = MasterKey.CKD($"{address.Host}:{address.Port}");
            if (!message.Verify(nodeKey.Address))
            {
                Logger.warning("Unverified message!");
                return;
            }

            // valid address?
            if (nodeKey.Address != address.Address)
            {
                Logger.warning("Invalid address");
                return;
            }

            // register peer
            Peers.Append(address);

            // session 
            lock (_sessionMap)
                _sessionMap[session.SID] = address.Address;

            lock (BConsole.Lock)
            {
                BConsole.Write(ConsoleColor.DarkCyan, address.ToString());
                BConsole.WriteLine(" registered!");
            }

            SyncAllPeers();
        }

        private void OnTcpMessageUnregister(ITcpSession session, Message message)
        {
            // unregister requested enode
            ElasticAddress address = message.Value<string>("enode");

            // verify message
            PrivateKey nodeKey = MasterKey.CKD($"{address.Host}:{address.Port}");
            if (!message.Verify(nodeKey.Address))
            {
                Logger.warning("Unverified message");
                return;
            }

            // valid address?
            if (nodeKey.Address != address.Address)
            {
                Logger.warning("Invalid address");
                return;
            }

            // remove peer
            Peers.Remove(address);

            // sync all peers
            SyncAllPeers();
        }

        private void OnTcpMessagePeek(ITcpSession session, Message message)
        {
            ElasticAddress address = message.Value<string>("enode");

            // verify message
            PrivateKey nodeKey = MasterKey.CKD($"{address.Host}:{address.Port}");
            if (!message.Verify(nodeKey.Address))
            {
                Logger.warning("Unverified message");
                return;
            }

            // valid address?
            if (nodeKey.Address != address.Address)
            {
                Logger.warning("Invalid address");
                return;
            }

            Message msg = new Message.Builder()
                .Body(ACTION, ACTION_PEEK)
                .Body("peers", Peers.Count)
                .Build(MasterKey);

            session?.Write(msg.ToBytes());
        }

        private void OnTcpMessageUpdate(ITcpSession session, Message message)
        {
            ElasticAddress address = message.Value<string>("enode");

            // verify message
            PrivateKey nodeKey = MasterKey.CKD($"{address.Host}:{address.Port}");
            if (!message.Verify(nodeKey.Address))
            {
                Logger.warning("Unverified message");
                return;
            }

            // valid address?
            if (nodeKey.Address != address.Address)
            {
                Logger.warning("Invalid address");
                return;
            }

            SyncPeers(session, Peers.Peers);
        }

        private void OnTcpMessageSync(ITcpSession session, Message message)
        {
            if (!message.Verify(MasterKey.Address))
            {
                Logger.warning("Unverified message");
                return;
            }

            // 모든 클라이언트에 업데이트
            SyncAllPeers(Peers.Peers);
        }

        public void SyncPeers(ITcpSession session, string[] catalogs)
        {
            Message message = new Message.Builder()
                .Body(ACTION, ACTION_UPDATE)
                .Body("peers", catalogs)
                .Build(MasterKey);

            session?.Write(message.ToBytes());
        }

        public void SyncAllPeers(string[] peers)
        {
            Message message = new Message.Builder()
                .Body(ACTION, ACTION_UPDATE)
                .Body("peers", peers)
                .Build(MasterKey);

            Server.SendAll(message.ToBytes());
        }

        public void SyncAllPeers()
        {
            SyncAllPeers(Peers.Peers);
        }

    }
}
