using BrylliteLib.Crypto;
using BrylliteLib.Hash;
using BrylliteLib.Net;
using BrylliteLib.Net.Elastic;
using BrylliteLib.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NodeServerApp
{
    public class NodeServer
    {
        public static readonly string TAG = "NodeServer";

        // Node Key
        private CPrivateKey mNodeKey;

        // Tracker client
        private TCPClient mTrackerClient;
        private string mTrackerHost;
        private int mTrackerPort;

        // P2P Host 
        private TCPServer mP2PHost;
        private ElasticAddress mNodeAddress;

        // NodeServer running?
        public bool Running = false;

        // Peer lists
        private Dictionary<string, ElasticAddress> mPeers = new Dictionary<string, ElasticAddress>();

        public NodeServer( CPrivateKey nodeKey )
        {
            // store NodeKey
            mNodeKey = nodeKey;

            // tracker client
            mTrackerClient = new TCPClient();
            mTrackerClient.OnConnect = OnTrackerConnect;
            mTrackerClient.OnClose = OnTrackerClose;
            mTrackerClient.OnMessage = OnTrackerMessage;

            // P2P Host
            mP2PHost = new TCPServer();
            mP2PHost.OnStart = OnHostStart;
            mP2PHost.OnStop = OnHostStop;
            mP2PHost.OnAccept = OnHostAccept;
            mP2PHost.OnClose = OnHostClose;
            mP2PHost.OnMessage = OnNodeMessage;
        }

        public ElasticAddress[] ToElasticAddressArray()
        {
            lock (mPeers)
            {
                return mPeers.Values.ToArray();
            }
        }

        public void Start( string host, int port, string tracker_host, int tracker_port )
        {
            // Node Elastic Address
            mNodeAddress = new ElasticAddress(mNodeKey.Address, host, port);
            Log.i(TAG, $"starting {mNodeAddress.ToString()}");

            // store tracker host:port
            mTrackerHost = tracker_host;
            mTrackerPort = tracker_port;

            // start p2p host
            mP2PHost.Start(host, port, 5);

            Running = mP2PHost.Running;
        }

        public void Stop()
        {
            // stop tracker client
            mTrackerClient.Stop();

            // stop p2p host
            mP2PHost.Stop();

            Running = mP2PHost.Running;
        }

        // connect tracker service timer
        private BrylliteTimer mTrackerTimer = new BrylliteTimer();

        public void Update()
        {
            // connect to tracker service if not connected
            if ( Running && !mTrackerClient.Connected && mTrackerTimer.TimeOut( 30 * 1000 ) )
            {
                Log.d(TAG, $"Connecting to Tracker Service! {mTrackerHost}:{mTrackerPort}");
                mTrackerClient.Start(mTrackerHost, mTrackerPort);
            }
        }

        private ElasticAddress FindElasticAddress( CAddress addr )
        {
            return (mPeers.ContainsKey(addr.HexAddress)) ? mPeers[addr.HexAddress] : null ;
        }

        private void SendTo( CAddress addr, byte[] data )
        {
            ElasticAddress target = FindElasticAddress(addr);
            if (target != null)
                SendTo(target, data);
        }

        private void SendTo( ElasticAddress target, byte[] data )
        {
            TCPClient.SendTo(target.Host, target.Port, data);
        }

        private async Task<bool> SendToAsync( ElasticAddress target, byte[] data )
        {
            return await TCPClient.SendToAsync(target.Host, target.Port, data);
        }

        private void SendAll( byte[] data )
        {
            foreach (var target in ToElasticAddressArray())
                SendTo(target, data);
        }

        private void SendAllAsync( byte[] data )
        {
            Task.Factory.StartNew(() =>
            {
                SendAll(data);
            });
        }

        private void SendAll( Payload data )
        {
            ElasticMessage message = new ElasticMessage(data, mNodeKey);
            SendAll(message.ToBytes());
        }

        private void OnHostStart(string host, int port)
        {
            Log.i(TAG, $"OnHostStart(): P2P Host started on {host}:{port}");
        }

        private void OnHostStop()
        {
            Log.i(TAG, $"OnHostStop()");
        }

        private void OnHostAccept(TCPSession session)
        {
        }

        private void OnHostClose(TCPSession session, int reason)
        {
        }


        private void OnNodeMessage( TCPSession session, byte[] bytes )
        {
            // message restore & verify
            ElasticMessage receivedMessage = ElasticMessage.Parse(bytes);
            if ( !receivedMessage.Verify())
            {
                Log.w(TAG, $"Node({mNodeAddress.HexAddress}) received unverified message!");
                return;
            }

            // sender exists on peer lists?
            ElasticAddress sender = FindElasticAddress(receivedMessage.FromAddress);
            if ( sender != null )
            {
                // is Elastic Grid Message?
                if (receivedMessage.To != null)
                {
                    OnElasticMessage(receivedMessage);
                    return;
                }

                // message for me
                // process here...
                Payload data = receivedMessage.Body;
                string command = data.Get<string>("command");
                switch( command )
                {
                    case "ping": OnNodeMessagePing(sender, data); return;
                    case "pong": OnNodeMessagePong(sender, data); return;
                    default: break;
                }

                Log.w(TAG, $"Node({mNodeAddress.HexAddress}) received unknown message!");
            }
        }

        private void OnTrackerConnect(TCPClient client, bool connected, string err)
        {
            if (!connected)
            {
                Log.d(TAG, $"OnTrackerConnect(): err={err}");
                return;
            }

            CSignature sign = mNodeKey.Sign(HashUtil.Hash256(mNodeKey.Address));

            // 피어 등록
            Payload msg = new Payload();
            msg.Set("command", "register");
            msg.Set("enode", mNodeAddress.ToString());
            msg.Set("sign", sign.ToByteArray());
            client.Send(msg.ToByteArray());
        }

        private void OnTrackerClose(TCPClient client, int reason)
        {
            Log.i(TAG, $"OnTrackerClose(): reason={reason}");

            // 트래커에서 끊어지면 종료하자
            Stop();
        }

        private void OnTrackerMessage(TCPClient client, byte[] data)
        {
            try
            {
                // 메세지 -> 피어 목록
                Payload msg = Payload.Parse(data);
                string command = msg.Get<string>("command");

                switch (command)
                {
                    case "broadcast": OnTrackerMessageBroadcast(client, msg); return;
                    case "update_peers": OnTrackerMessageUpdatePeers(client, msg); return;
                    default: break;
                }

                Log.i(TAG, $"OnTrackerMessage(): Message passed by unknown command({command})!");
            }
            catch (Exception e)
            {
                Log.w(TAG, $"OnTrackerMessage(): Exception! e={e.Message}, {e.Source}");
            }
        }

        private void OnTrackerMessageBroadcast(TCPClient client, Payload recvMsg)
        {
            int id = recvMsg.Get<int>("id");
            int size = recvMsg.Get<int>("size");

            try
            {
                // 랜덤 바이트
                byte[] rndBytes = RndGenerator.GetNonZeroBytes(size);
                Debug.Assert(size == rndBytes.Length);

                byte[] hash = HashUtil.Hash256(rndBytes);
                Debug.Assert(hash.Length == 32);

                // 전송할 메세지
                Payload data = new Payload();
                data.Set("command", "ping");
                data.Set("id", id);
                data.Set("hash", hash);
                data.Set("rndBytes", rndBytes);
                data.Set("msgTime", DateTime.Now);

                // 전체 전송
                ElasticSendAll(data);

                Log.i(TAG, $"Sending data(id:{id}, length:{rndBytes.Length})");

            }
            catch (Exception e)
            {
                Log.w(TAG, $"OnTrackerMessageConsensus(): e={e.Message}");
            }
        }

        private void OnTrackerMessageUpdatePeers(TCPClient client, Payload msg)
        {
            string[] peers = msg.Get<string[]>("peers");
            //            Log.i(TAG, $"OnTrackerMessage(): Updating Peer Lists! nPeers={peers.Length}");

            // 피어 목록 업데이트
            lock (mPeers)
            {
                mPeers.Clear();
                foreach (ElasticAddress addr in peers)
                {
                    if (!mPeers.ContainsKey(addr.HexAddress))
                        mPeers.Add(addr.HexAddress, addr);
                }
            }
        }

        public void ElasticSendAll(Payload data)
        {
            // 좌표계 결정
            ElasticLayout layout = ElasticLayout.DefineLayoutFor(mPeers.Count);
            if (layout.Count <= 1)
            {
                SendAll(data);
                return;
            }

            // 메세지 빌드
            ElasticMessage message = new ElasticMessage(data, mNodeKey);

            // Z-축 전송
            if (layout.Z > 1)
            {
                for (byte z = 1; z <= layout.Z; z++)
                {
                    ElasticCoordinates dest = new ElasticCoordinates(layout, new Elastic3D(0, 0, z));
                    message.To = dest;

                    // pick random target from give coordinates
                    ElasticAddress target = PickRndAddressFrom(dest);
                    if ( target != null )
                    {
                        SendTo(target, message.ToBytes());
                        Log.d(TAG, $"{mNodeAddress.HexAddress}: Sending message to {dest.ToString()}");
                    }
                }
            }
            // Y-축 전송
            else if (layout.Y > 1)
            {
                for (byte y = 1; y <= layout.Y; y++)
                {
                    ElasticCoordinates dest = new ElasticCoordinates(layout, new Elastic3D(0, y, 1));
                    message.To = dest;

                    // pick random target from give coordinates
                    ElasticAddress target = PickRndAddressFrom(dest);
                    if (target != null)
                    {
                        SendTo(target, message.ToBytes());
                        Log.d(TAG, $"{mNodeAddress.HexAddress}: Sending message to {dest.ToString()}");
                    }
                }
            }
            // X-축 전송
            else if (layout.X > 1)
            {
                for (byte x = 1; x <= layout.X; x++)
                {
                    ElasticCoordinates dest = new ElasticCoordinates(layout, new Elastic3D(x, 1, 1));
                    message.To = dest;

                    // pick random target from give coordinates
                    ElasticAddress target = PickRndAddressFrom(dest);
                    if (target != null)
                    {
                        SendTo(target, message.ToBytes());
                        Log.d(TAG, $"{mNodeAddress.HexAddress}: Sending message to {dest.ToString()}");
                    }
                }
            }
            else
            {
                throw new Exception("wtf!");
            }
        }

        private void OnElasticMessage(ElasticMessage receivedMessage)
        {
            ElasticCoordinates destination = receivedMessage.To;
            Elastic3D position = destination.To;
            ElasticLayout layout = destination.Layout;

            if (!position.Solid)
            {
                // Z-Coordinates must be filled.
                Debug.Assert(position.Z >= 1);

                // Y-축 전송
                if (position.Y <= 0)
                {
                    for (byte y = 1; y <= destination.Layout.Y; y++)
                    {
                        ElasticCoordinates dest = new ElasticCoordinates(layout, new Elastic3D(0, y, position.Z));
                        receivedMessage.To = dest;

                        ElasticAddress target = PickRndAddressFrom(dest);
                        if ( target != null )
                        {
                            SendTo(target, receivedMessage.ToBytes());
                            Log.d(TAG, $"{mNodeAddress.HexAddress}: relaying message to {dest.ToString()}");
                        }
                    }

                    return;
                }

                // X-축 전송
                for (byte x = 1; x <= destination.Layout.X; x++)
                {
                    ElasticCoordinates dest = new ElasticCoordinates(layout, new Elastic3D(x, position.Y, position.Z));
                    receivedMessage.To = dest;

                    ElasticAddress target = PickRndAddressFrom(dest);
                    if (target != null)
                    {
                        SendTo(target, receivedMessage.ToBytes());
                        Log.d(TAG, $"{mNodeAddress.HexAddress}: relaying message to {dest.ToString()}");
                    }
                }
            }
            else
            {
                // 직접 전송
                receivedMessage.To = null;

                // 해당 좌표의 모든 노드에 전달한다.
                ElasticAddress[] targets = FindElasticAddress(destination);
                foreach (var addr in targets)
                {
                    SendTo(addr, receivedMessage.ToBytes());
                }

                Log.d(TAG, $"{mNodeAddress.HexAddress}: broadcasting message to {position.ToString()}. nPeers={targets.Length}");
            }
        }

        // 해당 좌표에 포함된 엘라스틱 주소 목록을 구한다.
        private ElasticAddress[] FindElasticAddress(ElasticCoordinates coordinates)
        {
            ElasticLayout layout = coordinates.Layout;
            Elastic3D to = coordinates.To;

            ElasticAddress[] trackers = ToElasticAddressArray();
            List<ElasticAddress> addrs = new List<ElasticAddress>();
            foreach (var addr in trackers)
            {
                Elastic3D pos = layout.AddressToElastic3D(addr.Address);
                if (to.Contains(pos))
                    addrs.Add(addr);
            }

            return addrs.ToArray();
        }

        // pick a random elastic address from address lists of given coordinates
        // pick sender address if sender included on address lists
        private ElasticAddress PickRndAddressFrom( ElasticCoordinates coordinates )
        {
            ElasticAddress[] targets = FindElasticAddress(coordinates);
            if (targets.Length == 0) return null;

            foreach( var target in targets )
            {
                if (target.HexAddress == mNodeAddress.HexAddress)
                    return target;
            }

            return targets[RndGenerator.Next(targets.Length)];
        }

        private void OnNodeMessagePing(ElasticAddress sender, Payload msg)
        {
            int id = msg.Get<int>("id");
            byte[] hash = msg.Get<byte[]>("hash");
            byte[] rndBytes = msg.Get<byte[]>("rndBytes");
            DateTime msgTime = msg.Get<DateTime>("msgTime");

            Log.i(TAG, $"Received Data(id:{id}, length:{rndBytes.Length})");

            Debug.Assert(hash.SequenceEqual(HashUtil.Hash256(rndBytes)));

            // 잘 받았다고 응답 보낸다
            Payload ack = new Payload();
            ack.Set("command", "pong");
            ack.Set("id", id);
            ack.Set("hash", hash);
            ack.Set("msgTime", msgTime);

            try
            {
                ElasticMessage message = new ElasticMessage(ack, mNodeKey);
                SendTo(sender, message.ToBytes());
            }
            catch (Exception e)
            {
                Log.w(TAG, $"sending ack failed! sender={(string)sender}, e={e.Message}");
            }
        }

        private void OnNodeMessagePong(ElasticAddress sender, Payload msg)
        {
            int id = msg.Get<int>("id");
            byte[] hash = msg.Get<byte[]>("hash");
            DateTime msgTime = msg.Get<DateTime>("msgTime");
            TimeSpan travelTime = DateTime.Now - msgTime;
            double latency = travelTime.TotalMilliseconds;

            // 트래커 서비스에 리포트한다.
            Payload report = new Payload();
            report.Set("command", "report");
            report.Set("id", id);
            report.Set("sender", sender);
            report.Set("latency", latency);
            mTrackerClient.Send(report.ToByteArray());
        }
    }
}
