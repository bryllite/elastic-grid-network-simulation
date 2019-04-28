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
using Bryllite.Util.Log;
using Bryllite.Util.Payloads;
using Microsoft.Extensions.Configuration;

namespace Bryllite.App.ElasticNodeServiceApp
{
    public class PeerListClient
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

        // Node list provider
        private IPeerList Peers;

        // TCP client for catalog service
        private ITcpClient Client;

        private ElasticAddress ServerUrl;
        public PrivateKey MasterKey;

        private PrivateKey NodeKey;
        private ElasticAddress NodeUrl;

        public PeerListClient(IConfiguration configuration, ILoggable logger, IPeerList peers)
        {
            Configuration = configuration;
            Logger = logger;
            Peers = peers;

            ServerUrl = Configuration.GetValue<string>("ServerUrl");
            MasterKey = new PrivateKey(Configuration.GetValue<string>("MasterKey"));

            Client = new TcpClient()
            {
                OnConnect = OnTcpConnect,
                OnConnectFail = OnTcpConnectFail,
                OnDisconnect = OnTcpClose,
                OnMessage = OnTcpMessage
            };
        }

        public void Start(PrivateKey nodeKey, ElasticAddress nodeUrl)
        {
            NodeKey = nodeKey;
            NodeUrl = nodeUrl;

            Start();
        }

        public CancellationTokenSource cts = new CancellationTokenSource();
        private Task task;

        public void Start()
        {
            // start TCP client
            Client.Start(ServerUrl.Host, ServerUrl.Port);

            // reconnect 
            task = Task.Factory.StartNew(() =>
            {
                OnMain();
            });
        }

        public void Stop()
        {
            Client.Stop();
        }

        private void OnMain()
        {
            while (!cts.IsCancellationRequested)
            {
                Thread.Sleep(5000);

                if (Client.State == TcpClient.ConnectState.Disconnected )
                    Client.Start(ServerUrl.Host, ServerUrl.Port);
            }
        }

        public void OnTcpConnect(ITcpClient client)
        {
            Logger.info($"PeerListService connected");

            Register(client.Session);
        }

        public void OnTcpConnectFail(ITcpClient client, string err)
        {
            Logger.warning("can't connect to PeerListService", err);
        }

        public void OnTcpClose(ITcpClient client, int reason)
        {
            Logger.debug($"PeerListService disconnected");
        }

        public void OnTcpMessage(ITcpClient client, byte[] data)
        {
            Message message = Message.Parse(data);
            switch (message.Value<string>(ACTION))
            {
                case ACTION_UPDATE: OnActionUpdate(client.Session, message); return;
                default: break;
            }

            Logger.warning($"Unknown Action! action={message.Value<string>(ACTION)}");
        }

        private void OnActionUpdate(ITcpSession session, Message message)
        {
            if (!message.Verify(MasterKey.Address))
                Logger.warning($"MasterKey.Address verify failed");

            // peer list
            string[] peers = message.Value<string[]>("peers");

            // update catalogs
            Peers.Update(peers);

//            Logger.debug($"PeerList UPDATED! peers={peers.Length}");
        }

        private void Register(ITcpSession session)
        {
            Message message = new Message.Builder()
                .Body(ACTION, ACTION_REGISTER)
                .Body("enode", NodeUrl.ToString())
                .Build(NodeKey);

            session?.Write(message.ToBytes());
        }

        public void RequestSync()
        {
            Message message = new Message.Builder()
                .Body(ACTION, ACTION_SYNC)
                .Build(MasterKey);

            SendMessage(message.ToBytes());
        }

        public void SendMessage(byte[] data)
        {
            Client?.Send(data);
        }
    }
}
