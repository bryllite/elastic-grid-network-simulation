using BrylliteLib.Crypto;
using BrylliteLib.Hash;
using BrylliteLib.Net;
using BrylliteLib.Net.Elastic;
using BrylliteLib.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TrackerServiceApp
{
    public class TrackerService
    {
        public static readonly string TAG = "TrackerService";

        // TCP Server for tracker service
        private TCPServer _tcp_server;

        // peer lists
        private Dictionary<ulong, ElasticAddress> _peers = new Dictionary<ulong, ElasticAddress>();

        // test reports
        private Dictionary<int, Report> _reports = new Dictionary<int, Report>();

        // need to update tracker ?
        private bool _need_to_update_tracker = false;
        private BrylliteTimer _update_tracker_timer = new BrylliteTimer();

        public bool Running
        {
            get
            {
                return _tcp_server.Running;
            }
        }

        public ElasticAddress[] ToElasticAddressArray()
        {
            lock( _peers )
            {
                return _peers.Values.ToArray();
            }
        }

        public TrackerService()
        {
            _tcp_server = new TCPServer();
            _tcp_server.OnStart = OnTCPServerStart;
            _tcp_server.OnStop = OnTCPServerStop;
            _tcp_server.OnAccept = OnTCPServerAccept;
            _tcp_server.OnClose = OnTCPServerClose;
            _tcp_server.OnMessage = OnTCPServerMessage;
        }

        public void Start( string host, int port, int acceptThreadCount = 16 )
        {
            // start tracker service
            _tcp_server.Start(host, port, acceptThreadCount);
        }

        public void Stop()
        {
            _tcp_server.Stop();
        }

        private int _block_size;
        private int _times;
        private int _time_limit;

        public void RunTest(int nBlockSize, int nTimes, int nTimeLimit)
        {
            _block_size = nBlockSize;
            _times = nTimes;
            _time_limit = nTimeLimit;

            Task.Factory.StartNew(() =>
            {
                Log.d(TAG, $"STARTING TEST! nBlockSize={nBlockSize}, nTimes={nTimes}, nTimeLimit={nTimeLimit}");

                _reports.Clear();

                Thread.Sleep(1000);

                Report report;
                for (int id = 0; id < nTimes; id++)
                {
                    Log.d(TAG, $"Sending message({id})...");

                    // 보고서 생성
                    report = new Report(id, _peers.Count, nBlockSize);
                    lock (_reports)
                    {
                        _reports[id] = report;
                    }

                    // 블록을 전파할 랜덤 노드 선택
                    TCPSession target = null;
                    TCPSession[] targets = _tcp_server.Sessions.Shuffle();
                    if (targets.Length > 0)
                        target = targets[RndGenerator.Next(targets.Length)];

                    if (target == null)
                    {
                        Thread.Sleep(nTimeLimit);
                        continue;
                    }

                    // 메세지
                    Payload msg = new Payload();
                    msg.Set("command", "broadcast");
                    msg.Set("id", id);
                    msg.Set("size", nBlockSize);

                    // 메세지 전송
                    _tcp_server.SendTo(target, msg.ToByteArray());

                    // 모두 수신하거나, 제한 시간 동안 대기
                    DateTime stime = DateTime.Now;
                    while ((DateTime.Now - stime).TotalMilliseconds < nTimeLimit && !report.ReceivedAll)
                    {
                        Thread.Sleep(10);
                    }

                    // 이번 메세지 결과
                    if (report.ReceivedAll)
                    {
                        Log.v(TAG, $"message({id}) travel completed! received={report.ReceivedCount}, latency=[{report.MinTravelTime},{report.AvgTravelTime},{report.MaxTravelTime}](ms)");
                    }
                    else
                    {
                        Log.w(TAG, $"message({id}) travel missed! received={report.ReceivedCount}, latency=[{report.MinTravelTime},{report.AvgTravelTime},{report.MaxTravelTime}](ms)");
                    }
                }

                // 결과 보고
                Report(nBlockSize, _peers.Count, nTimes, nTimeLimit);
            });
        }

        private void Report(int nBlockSize, int nPeers, int nTimes, int nTimeLimit)
        {
            // 결과 보고
            Log.d(TAG, $"[TEST REPORT]");
            int sum = 0;
            int max = 0, min = int.MaxValue;
            Report[] reports = _reports.Values.ToArray();
            foreach (Report report in reports)
            {
                sum += report.AvgTravelTime;
                max = Math.Max(max, report.MaxTravelTime);
                min = Math.Min(min, report.MinTravelTime);
                Log.i(TAG, $"{report.ToString()}");
            }

            int latency = reports.Length == 0 ? 0 : sum / reports.Length;
            Log.i(TAG, $"Latency={{{min},{latency},{max}}}(ms), nBlockSize={nBlockSize}, nPeers={nPeers}, nTimes={nTimes}, nTimeLimit={nTimeLimit}");
        }

        public void Update()
        {
            // 1초에 한번씩 트래커 업데이트 체크
            if (_update_tracker_timer.TimeOut(1 * 1000))
                UpdateTrackers();
        }

        public void Report()
        {
            Report(_block_size, _peers.Count, _times, _time_limit);
        }

        public void Information()
        {
            int nPeers = _peers.Count;
            int nSession = _tcp_server.SessionCount;
            ElasticLayout layout = ElasticLayout.DefineLayoutFor(nPeers);

            Log.i(TAG, $"[Information]");
            Log.i(TAG, $"nPeers = {nPeers}");
            Log.i(TAG, $"nSession = {nSession}");
            Log.i(TAG, $"Layout = {layout.ToString()}");

            int[,,] counts = new int[layout.X + 1, layout.Y + 1, layout.Z + 1];
            foreach (ElasticAddress addr in ToElasticAddressArray())
            {
                Elastic3D pos = layout.AddressToElastic3D(addr.Address);
                counts[pos.X, pos.Y, pos.Z]++;
            }

            for (int x = 1; x <= layout.X; x++)
            {
                for (int y = 1; y <= layout.Y; y++)
                {
                    for (int z = 1; z <= layout.Z; z++)
                    {
                        Log.d(TAG, $"Layout[{x},{y},{z}].nPeers={counts[x, y, z]}");
                    }
                }
            }
        }

        private void OnTCPServerStart(string host, int port)
        {
            Log.i(TAG, $"OnTCPServerStart(): host={host}, port={port}");
        }

        private void OnTCPServerStop()
        {
            Log.i(TAG, $"OnTCPServerStop()");
        }

        private void OnTCPServerAccept(TCPSession session)
        {
            //            Log.i(TAG, $"OnTCPServerAccept(): session={session.ID}");
        }

        private void OnTCPServerClose(TCPSession session, int reason)
        {
            Log.i(TAG, $"OnTCPServerClose(): session={session.ID}");

            // 접속 종료된 세션의 피어 목록을 제거할까?
            lock (_peers)
            {
                _peers.Remove(session.ID);
                _need_to_update_tracker = true;
            }
        }

        private void OnTCPServerMessage(TCPSession session, byte[] data)
        {
            //            Log.i(TAG, $"OnTCPServerMessage(): session={session.ID}, data.Length={data.Length}");

            try
            {
                Payload msg = Payload.Parse(data);
                string command = msg.Get<string>("command");
                switch (command)
                {
                    case "register": OnTCPServerMessageRegister(session, msg); return;
                    case "report": OnTCPServerMessageReport(session, msg); return;
                    default:
                        break;
                }

                Log.w(TAG, $"OnTCPServerMessage(): Message passed by Unknown command({command})");
            }
            catch (Exception e)
            {
                Log.w(TAG, $"OnTCPServerMessage(): Exception! e={e.Message}");
                return;
            }
        }

        private void OnTCPServerMessageRegister(TCPSession session, Payload msg)
        {
            try
            {
                string enode = msg.Get<string>("enode");
                CSignature sign = CSignature.FromByteArray(msg.Get<byte[]>("sign"));

                // 노드 사인 검증
                ElasticAddress eNode = new ElasticAddress(enode);
                byte[] hash = HashUtil.Hash256(eNode.Address);
                CPublicKey publicKey = sign.ToPublicKey(hash);
                if (!sign.Verify(hash) || eNode.HexAddress != publicKey.HexAddress)
                {
                    Log.w(TAG, $"bnode({enode}) sign not verified!");
                    return;
                }

                // 피어 목록에 추가
                lock (_peers)
                {
                    if (!_peers.ContainsKey(session.ID) && !_peers.ContainsValue(eNode))
                    {
                        _peers.Add(session.ID, eNode);
                        _need_to_update_tracker = true;
                        Log.i(TAG, $"{enode} registered! nPeers={_peers.Count}");
                    }
                }
            }
            catch (Exception e)
            {
                Log.w(TAG, $"OnTCPServerMessageRegister(): Exception! e={e.Message}");
            }
        }

        private void OnTCPServerMessageReport(TCPSession session, Payload msg)
        {
            int id = msg.Get<int>("id");
            double latency = msg.Get<double>("latency");

            // 보고서 갱신
            lock (_reports)
            {
                _reports[id].Add(latency);
            }
        }


        public void UpdateTrackers()
        {
            if (_need_to_update_tracker)
            {
                List<string> peers = new List<string>();
                ElasticAddress[] addrs = ToElasticAddressArray();
                foreach( var addr in addrs )
                    peers.Add(addr.ToString());

                Log.d(TAG, $"Trackers updating... nPeers={peers.Count}");

                Payload msg = new Payload();
                msg.Set("command", "update_peers");
                msg.Set("peers", peers.ToArray());

                _tcp_server.SendAll(msg.ToByteArray());

                _need_to_update_tracker = false;
            }
        }

    }
}
