using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Bryllite.Core.Key;
using Bryllite.Net.Messages;
using Bryllite.Net.Tcp;
using Bryllite.Util;
using Bryllite.Util.Log;
using Bryllite.Util.Payloads;
using Microsoft.Extensions.Configuration;

namespace Bryllite.Net.Elastic
{
    public class ElasticNode : IElasticNode
    {
        private IConfiguration Configuration;
        private ILoggable Logger;
        private ElasticAddress LocalEndPoint;

        private PrivateKey NodeKey;

        // TCP server for node service
        private TcpServer TcpServer;

        // Peer List Provider
        private IPeerList Peers;

        private CancellationTokenSource cts;

        // message handler
        public Action<ElasticAddress, Message> OnMessage { get; set; }


        public ElasticNode( IConfiguration configuration, ILoggable logger, IPeerList peers, PrivateKey nodeKey, ElasticAddress localEndPoint )
        {
            Configuration = configuration;
            Logger = logger;
            NodeKey = nodeKey;
            LocalEndPoint = localEndPoint;
            Peers = peers;

            TcpServer = new TcpServer()
            {
                OnStart = OnTcpServerStart,
                OnStop = OnTcpServerStop,
                OnAccept = OnTcpServerAccept,
                OnClose = OnTcpServerClose,
                OnMessage = OnTcpServerMessage
            };

        }

        public void Start( CancellationTokenSource cts )
        {
            this.cts = cts;

            int acceptThreadWorkers = Configuration.GetValue("acceptThreadWorkers", 16);

            // TCP server start
            TcpServer.Start(LocalEndPoint.Host, LocalEndPoint.Port, acceptThreadWorkers);
        }

        public void Stop()
        {
            cts.Cancel();

            TcpServer.Stop();
        }

        private void OnTcpServerStart(string host, int port)
        {
            Logger.info($"ElasticNode.OnTcpServerStart({host}, {port})");
        }

        private void OnTcpServerStop()
        {
            Logger.info($"ElasticNode.OnTcpServerStop()");
        }

        private void OnTcpServerAccept(ITcpSession session)
        {
        }

        private void OnTcpServerClose(ITcpSession session, int reason)
        {
        }

        private void OnTcpServerMessage(ITcpSession session, byte[] data)
        {
            // valid sign?
            Message message = Message.Parse(data);
            if (!message.Verify())
            {
                Logger.warning($"Message verify() failed");
                return ;
            }

            // authorized peer?
            ElasticAddress peer = Peers.Find(message.Sender);
            if (ReferenceEquals(peer, null))
            {
                Logger.warning($"Message from unknown peer");
                return ;
            }

            // should route this message?
            if (message.ShouldRoute())
            {
                OnElasticMessage(message);
                return;
            }

            // message handler invoke
            OnMessage?.Invoke( peer, message );
        }

        public static bool SendTo(byte[] data, string host, int port)
        {
            return SendTo(data, host, port, BLog.Global);
        }

        // Send packet to host
        // connect -> send -> close
        public static bool SendTo(byte[] data, string host, int port, ILoggable logger)
        {
            Debug.Assert(!string.IsNullOrEmpty(host) && data.Length > 0);

            // 패킷 최대 크기
            Debug.Assert(data.Length < TcpSession.MAX_PACKET_SIZE - TcpSession.HEADER_SIZE);
            if (data.Length > TcpSession.MAX_PACKET_SIZE - TcpSession.HEADER_SIZE)
            {
                logger.error($"exceeds MAX_PACKET_SIZE({TcpSession.MAX_PACKET_SIZE})");
                return false;
            }

            TcpClient client = null;
            try
            {
                client = new TcpClient(host, port);
            }
            catch (Exception e)
            {
                logger.warning($"can't connect to Peer! remote={host}:{port}, e={e.Message}");
                return false;
            }

            // 전송할 데이터
            List<byte> bytes = new List<byte>();
            bytes.AddRange(BitConverter.GetBytes(data.Length));
            bytes.AddRange(data);
            byte[] bytesToSend = bytes.ToArray();

            bool ret = false;
            try
            {
                // write data to socket
                NetworkStream ns = client.GetStream();
                ns.Write(bytesToSend, 0, bytesToSend.Length);

                ret = true;
            }
            catch (Exception e)
            {
                logger.error($"can't write to peer! remote={host}:{port}, data.Length={data.Length}, e={e.Message}");
            }
            finally
            {
                // 소켓 종료
                client.Close();
            }

            return ret;
        }

        public bool SendTo(byte[] data, ElasticAddress peer)
        {
            return SendTo(data, peer.Host, peer.Port);
        }

        public bool SendTo(Message message, ElasticAddress peer)
        {
            return SendTo(message.ToBytes(), peer);
        }

        public int SendTo(Message message, ElasticAddress[] peers)
        {
            byte[] data = message.ToBytes();

            int sent = 0;
            foreach (var peer in peers)
                if (SendTo(data, peer)) sent++;

            return sent;
        }

        private void RouteTo(Message message, Elastic3D to)
        {
            // peer lists
            ElasticAddress[] peers = Peers.ToArray<ElasticAddress>(to, message.Layout()).Shuffle();

            // send me if peers contains me
            if (peers.Contains(LocalEndPoint))
            {
                new BConsole.MessageBuilder()
                    .Append("(+) Routing message(")
                    .Append(ConsoleColor.DarkCyan, message.ID.Ellipsis())
                    .Append(") To:")
                    .Append(ConsoleColor.DarkGreen, to)
                    .Append(", Me:")
                    .WriteLine(ConsoleColor.DarkGreen, LocalEndPoint.Ellipsis());

                SendTo(message, LocalEndPoint);

                return;
            }

            // send message to randomly picked one
            // try next if failed
            foreach (var peer in peers)
            {
                new BConsole.MessageBuilder()
                    .Append("(+) Routing message(")
                    .Append(ConsoleColor.DarkCyan, message.ID.Ellipsis())
                    .Append(") To:")
                    .Append(ConsoleColor.DarkGreen, to)
                    .Append(", Peer:")
                    .WriteLine(ConsoleColor.DarkGreen, peer.Ellipsis());

                if (SendTo(message, peer))
                    break;
            }
        }

        public void SendAll(Message message)
        {
            SendAll(message, ElasticLayout.N);
        }

        public void SendAll(Message message, byte n)
        {
            // define layout
            ElasticLayout layout = ElasticLayout.DefineLayout(Peers.Count, n);
            if (layout.Mul() <= 1)
            {
                ElasticAddress[] peers = Peers.ToArray<ElasticAddress>();

                new BConsole.MessageBuilder()
                    .Append("(!) Broadcasting message(")
                    .Append(message.ID.Ellipsis())
                    .Append("), To=")
                    .Append(ConsoleColor.DarkGreen, layout)
                    .Append(", nPeers=")
                    .WriteLine(ConsoleColor.DarkGreen, peers.Length);

                SendTo(message, peers);
                return;
            }

            // this node coordinates
            Elastic3D me = layout.DefineCoordinates(NodeKey.Address);

            // send message to z-axis
            if (layout.Z > 1)
            {
                for (byte z = 1; z <= layout.Z; z++)
                {
                    Elastic3D to = new Elastic3D(0, 0, z);
                    message.RouteTo(3, to, layout, NodeKey);

                    RouteTo(message, to);
                }

                return;
            }

            // send message to y-axis
            if (layout.Y > 1)
            {
                for (byte y = 1; y <= layout.Y; y++)
                {
                    Elastic3D to = new Elastic3D(0, y, 1);
                    message.RouteTo(2, to, layout, NodeKey);

                    RouteTo(message, to);
                }

                return;
            }

            // send message to x-axis
            for (byte x = 1; x <= layout.X; x++)
            {
                Elastic3D to = new Elastic3D(x, 1, 1);
                message.RouteTo(1, to, layout, NodeKey);

                RouteTo(message, to);
            }
        }


        private void OnElasticMessage(Message message)
        {
            // routes info
            byte ttl = message.TimeToLive();
            Elastic3D to = message.To();
            ElasticLayout layout = message.Layout();
            Elastic3D me = layout.DefineCoordinates(NodeKey.Address);

            // verify message
            if (!message.VerifyRouter())
            {
                Logger.warning("router verify() failed");
                return ;
            }

            // router permitted?
            if (!Peers.Exists(message.Router()))
            {
                Logger.warning("unknown router!");
                return ;
            }

            new BConsole.MessageBuilder()
                .Append("(-) Received ShouldRoute message(")
                .Append(ConsoleColor.DarkCyan, message.ID.Ellipsis())
                .Append(") To:")
                .WriteLine(ConsoleColor.DarkGreen, to);

            // broadcast to all cell
            if (to.Mul() > 0 && ttl == 1 )
            {
                ElasticAddress[] peers = Peers.ToArray<ElasticAddress>(to, layout);

                new BConsole.MessageBuilder()
                    .Append("(!) Broadcasting message(")
                    .Append(ConsoleColor.DarkCyan, message.ID.Ellipsis() )
                    .Append("), To=")
                    .Append(ConsoleColor.DarkGreen, to)
                    .Append(", nPeers=")
                    .WriteLine(ConsoleColor.DarkGreen, peers.Length);

                // 해당 좌표의 모든 노드에 전송한다.
                message.RouteTo(0, to, layout, NodeKey);
                SendTo(message, peers);
                return ;
            }

            // z-axis must be > 0
            if (to.Z < 1)
            {
                Logger.error( $"to.z < 1");
                return ;
            }

            // y-axis
            if ( ttl == 3 )
            {
                for (byte y = 1; y <= layout.Y; y++)
                {
                    to.Y = y;
                    message.RouteTo(2, to, layout, NodeKey);
                    RouteTo(message, to);
                }
            }
            // x-axis
            else if ( ttl == 2 && to.Y > 0 )
            {
                for (byte x = 1; x <= layout.X; x++)
                {
                    to.X = x;
                    message.RouteTo(1, to, layout, NodeKey);
                    RouteTo(message, to);
                }
            }
        }
    }
}
