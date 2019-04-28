using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Bryllite.Core.Key;
using Bryllite.Net.Elastic;
using Bryllite.Net.Messages;
using Bryllite.Util;
using Bryllite.Util.Log;
using Bryllite.Util.Payloads;
using Microsoft.Extensions.Configuration;

namespace Bryllite.App.ElasticNodeServiceApp
{
    public class ElasticGridNetworkTests
    {
        //private static readonly ConsoleColor Red = ConsoleColor.Red;
        private static readonly ConsoleColor DarkRed = ConsoleColor.DarkRed;
        //private static readonly ConsoleColor Green = ConsoleColor.Green;
        private static readonly ConsoleColor DarkGreen = ConsoleColor.DarkGreen;
        private static readonly ConsoleColor Blue = ConsoleColor.Blue;
        //private static readonly ConsoleColor DarkBlue = ConsoleColor.DarkBlue;
        //private static readonly ConsoleColor Yellow = ConsoleColor.Yellow;
        private static readonly ConsoleColor DarkYellow = ConsoleColor.DarkYellow;
        private static readonly ConsoleColor Cyan = ConsoleColor.Cyan;
        private static readonly ConsoleColor DarkCyan = ConsoleColor.DarkCyan;
        //private static readonly ConsoleColor Magenta = ConsoleColor.Magenta;
        //private static readonly ConsoleColor DarkMagenta = ConsoleColor.DarkMagenta;
        //private static readonly ConsoleColor White = ConsoleColor.White;
        //private static readonly ConsoleColor Gray = ConsoleColor.Gray;
        //private static readonly ConsoleColor DarkGray = ConsoleColor.DarkGray;
        //private static readonly ConsoleColor Black = ConsoleColor.Black;

        public static readonly byte DEFAULT_N = 8;
        public static readonly int DEFAULT_NPEERS = 32;
        public static readonly int DEFAULT_MSGKBYTES = 128;
        public static readonly int DEFAULT_NTIMES = 10;

        private ILoggable Logger;
        private IElasticNode Node;
        private IPeerList Peers;

        private PrivateKey NodeKey;

        private TestReports Reports = new TestReports();

        public string TimeCode => LogExtension.TimeCode;

        // TEST environment values
        public Payload Env = new Payload.Builder()
            .Value("n", DEFAULT_N)
            .Value("nTimes", DEFAULT_NTIMES)
            .Value("msgKBytes", DEFAULT_MSGKBYTES)
            .Value("nPeers", DEFAULT_NPEERS)
            .Build();

        public ElasticGridNetworkTests( ILoggable logger, IElasticNode node, IPeerList peers, PrivateKey nodeKey )
        {
            Logger = logger;
            Node = node;
            Peers = peers;
            NodeKey = nodeKey;
        }

        ~ElasticGridNetworkTests()
        {
            KillAll();
        }

        private CancellationTokenSource cts;
        public void Start()
        {
            cts = new CancellationTokenSource();

            Task.Factory.StartNew(() =>
            {
                Main();
            });
        }

        public void Stop()
        {
            cts?.Cancel();
        }

        public void Main()
        {
            byte n = Env.Value("n", DEFAULT_N);
            int nTimes = Env.Value("nTimes", DEFAULT_NTIMES);
            int msgKBytes = Env.Value("msgKBytes", DEFAULT_MSGKBYTES);

            int nPeers = Peers.Count;
            int nonce = 0;
            int waitTime = Math.Min( 60000, Math.Max( 1000, nPeers * 32 ) );

            new BConsole.MessageBuilder()
                .Append(DarkGreen, $"[{TimeCode}] ")
                .Append(Blue, ">> TEST STARTED!")
                .Append(" n=")
                .Append(DarkGreen, n)
                .Append(", nPeers=")
                .Append(DarkGreen, nPeers)
                .Append(", nTimes=")
                .Append(DarkGreen, nTimes)
                .Append(", msgKBytes=")
                .Append(DarkGreen, msgKBytes)
                .Append(", waitTime=")
                .Append(DarkGreen, waitTime)
                .WriteLine("(ms)");

            // 결과 보고서 초기화
            Reports.Clear();

            // 5초에 한번씩 전체 메세지 전송한다.
            Stopwatch sw = Stopwatch.StartNew();
            while (!cts.IsCancellationRequested && nonce++ < nTimes)
            {
                byte[] rndBytes = RndProvider.GetNonZeroBytes(msgKBytes * 1024);

                // message
                Message message = new Message.Builder()
                    .Action("ping")
                    .Body("nonce", nonce)
                    .Body("rndBytes", rndBytes)
                    .Build(NodeKey);

                ElasticLayout layout = ElasticLayout.DefineLayout(Peers.Count, n);
                Elastic3D me = layout.DefineCoordinates(NodeKey.Address);

                // new report item for this message
                TestReports.ReportItem report = Reports.NewItem(message, Peers.Count);

                new BConsole.MessageBuilder()
                    .Append(">> Sending message[nonce=")
                    .Append(Cyan, nonce)
                    .Append("](")
                    .Append(DarkCyan, message.ID.Ellipsis())
                    .Append("): message.length=")
                    .Append(DarkGreen, message.Length)
                    .Append(", n=")
                    .Append(DarkGreen, n)
                    .Append(", layout=")
                    .Append(DarkGreen, layout)
                    .Append(", host.coordinates=")
                    .Append(DarkGreen, me)
                    .WriteLine();

                // send message to all peers
                SendAll(message, n);

                // wait for send message complete
                sw.Restart();
                while (!report.ReceivedAll && sw.ElapsedMilliseconds < waitTime * 3 && !cts.IsCancellationRequested ) Thread.Sleep(10);

                // message result
                new BConsole.MessageBuilder()
                    .Append(">> message[nonce=")
                    .Append(Cyan, nonce)
                    .Append("](")
                    .Append(DarkCyan, message.ID.Ellipsis())
                    .Append(") result=")
                    .WriteLine(report.ToString());

                // waiting for next message
                if (nonce < nTimes)
                {
                    BConsole.WriteLine( ConsoleColor.White, "Waiting for next message...");
                    while (waitTime - (int)sw.ElapsedMilliseconds > 0 && !cts.IsCancellationRequested ) Thread.Sleep(10);
                }
            }

            Report();
        }


        public void SendAll(Message message, byte n)
        {
            Node.SendAll(message, n);
        }

        public void SendTo(Message message, ElasticAddress peer)
        {
            Node.SendTo(message, peer);
        }

        public void OnMessagePing(ElasticAddress sender, Message messagePing)
        {
            Message ack = new Message.Builder()
                .Action("pong")
                .Body("msgHash", messagePing.ID )
                .Body("msgBytes", messagePing.Length)
                .Body("msgTime", messagePing.TimeStamp)
                .Build(NodeKey);

            SendTo(ack, sender);
        }

        public void OnMessagePong(ElasticAddress sender, Message messagePong)
        {
            string messageId = messagePong.Value<string>("msgHash");
            Reports.AddItem(messageId, sender);
        }

        public void Report()
        {
            Reports.ShowReports();
        }

        public void ClearReport()
        {
            Reports.Clear();
        }

        public static string BinFilePath => Assembly.GetEntryAssembly().Location;
        public static string BinFileName => Path.GetFileName(BinFilePath);
        public static string BinPath => Path.GetDirectoryName(BinFilePath);
        public static string BinFileExt => Path.GetExtension(BinFilePath).ToLower();

        // 프로세스 목록
        private Dictionary<string, Process> _procs = new Dictionary<string, Process>();

        private bool Fork(string host, int port)
        {
            string key = $"{host}:{port}";
            string arguments = $"--host={host} --port={port}";

            if (_procs.ContainsKey(key)) return false;

            Process process = new Process();
            if (Path.GetExtension(BinFileName).ToLower() == ".dll")
            {
                process.StartInfo.FileName = "dotnet";
                process.StartInfo.Arguments = $"{BinFileName} {arguments}";
            }
            else
            {
                process.StartInfo.FileName = BinFileName;
                process.StartInfo.Arguments = arguments;
            }
            process.StartInfo.WorkingDirectory = BinPath;
            process.StartInfo.CreateNoWindow = true;
            process.StartInfo.UseShellExecute = false;
            process.Exited += (sender, e) =>
            {
                Logger.warning($"process({key}) has exited! exitCode={((Process)sender).ExitCode}");
                _procs.Remove(key);
            };

            bool started = process.Start();
            if (started)
                _procs[key] = process;

            return started;
        }

        public void Prepare(string host, int basePort)
        {
            int nPeers = Env.Value("nPeers", DEFAULT_NPEERS);

            Logger.info($"Preparing ({nPeers}) peers...");

            for (int i = 1; i < nPeers; i++)
            {
                int port = basePort + i;

                if (!Fork(host, port))
                    continue;

                Logger.debug($"peer({host}:{port}) process started");
                while (!Peers.Exists(host, port)) Thread.Sleep(10);
            }

            Logger.info($"({Peers.Count}) peers started!");
        }


        public void KillAll()
        {
            Logger.info("Killing forked process...");

            lock (_procs)
            {
                foreach (var proc in _procs.Values)
                {
                    try
                    {
                        proc.Kill();
                        proc.WaitForExit();
                    }
                    catch (Exception e)
                    {
                        Logger.error($"exception! e={e.Message}");
                    }
                }

                _procs.Clear();
            }

            Logger.info("Killing forked process terminated");
        }

        public void Kill(int port)
        {
            foreach (var pair in _procs.ToArray())
            {
                try
                {
                    string key = pair.Key;
                    if (port == Convert.ToInt32(key.Split(":")[1]))
                    {
                        var proc = pair.Value;
                        proc.Kill();
                        proc.WaitForExit();

                        lock (_procs)
                        {
                            _procs.Remove(key);
                        }

                        Logger.info($"process({key}) terminated");
                        return;
                    }
                }
                catch (Exception e)
                {
                    Logger.error($"exception! e={e.Message}");
                }
            }
        }

        public class TestReports
        {
            public Dictionary<string, ReportItem> _reports = new Dictionary<string, ReportItem>();

            public class ReportItem
            {
                public string MessageId => _message?.ID;
                public int Peers => _peers;
                public long MessageTime = DateTime.Now.Ticks;

                public int Received => _received.Count;
                public decimal ReceiveRate => Peers > 0 ? decimal.Round(Received * 100 / Peers) : 0;

                public bool ReceivedAll => _received.Count >= Peers;

                public long AverageLatency => _received.Count > 0 ? (long)_received.Values.Average() : 0;
                public long MaxLatency => _received.Count > 0 ? _received.Values.Max() : 0;
                public long LowLatency => _received.Count > 0 ? _received.Values.Min() : 0;

                public long SumLatency => _received.Count > 0 ? _received.Values.Sum() : 0;

                private Dictionary<ElasticAddress, long> _received = new Dictionary<ElasticAddress, long>();

                private Message _message;
                private int _peers;

                public ReportItem(Message message, int peers)
                {
                    _message = message;
                    _peers = peers;
                }

                public void Append(ElasticAddress peer)
                {
                    _received[peer] = ( DateTime.Now.Ticks - MessageTime ) / 10000 ;
                }

                public override string ToString()
                {
                    decimal rate = decimal.Round(Received * 100 / Peers, 2);
                    return $"Received: {Received}/{Peers} ({rate}%), Low: {LowLatency}(ms), Avg: {AverageLatency}(ms), Max: {MaxLatency}(ms)";
                }
            }


            public TestReports()
            {
            }

            public void Clear()
            {
                lock (_reports)
                    _reports.Clear();
            }

            public ReportItem NewItem(Message message, int peers)
            {
                ReportItem report = new ReportItem(message, peers);

                lock (_reports)
                {
                    if (!_reports.ContainsKey(message.ID))
                        _reports[message.ID] = report;
                }

                return report;
            }

            public void AddItem(string messageId, ElasticAddress peer)
            {
                lock (_reports)
                {
                    if (_reports.ContainsKey(messageId))
                        _reports[messageId].Append(peer);
                }
            }

            public void ShowReports()
            {
                BConsole.WriteLine();
                BConsole.WriteLine(Blue, "REPORTS:");

                long ReceivedSum = 0, SentSum = 0, LowSum = 0, AvgSum = 0, MaxSum = 0;
                foreach (var report in _reports.Values.ToArray())
                {
                    new BConsole.MessageBuilder()
                        .Append("  message(")
                        .Append(DarkCyan, report.MessageId.Ellipsis())
                        .Append(") received: ")
                        .Append(report.Received==report.Peers?DarkGreen:DarkRed, report.Received)
                        .Append("/")
                        .Append(DarkGreen, report.Peers)
                        .Append(" (")
                        .Append(report.ReceiveRate==100m?DarkGreen:DarkRed, report.ReceiveRate)
                        .Append("%), Low: ")
                        .Append(report.LowLatency<=500?DarkGreen:report.LowLatency<=5000?DarkYellow:DarkRed, report.LowLatency)
                        .Append("(ms), Avg: ")
                        .Append(report.AverageLatency<=500?DarkGreen:report.AverageLatency<=5000?DarkYellow:DarkRed, report.AverageLatency)
                        .Append("(ms), Max: ")
                        .Append(report.MaxLatency<=500?DarkGreen:report.MaxLatency<=5000?DarkYellow:DarkRed, report.MaxLatency)
                        .WriteLine("(ms)");

                    ReceivedSum += report.Received;
                    SentSum += report.Peers;
                    LowSum += report.LowLatency;
                    AvgSum += report.AverageLatency;
                    MaxSum += report.MaxLatency;
                }

                decimal ReceiveRate = SentSum > 0 ? decimal.Round(ReceivedSum * 100 / SentSum, 2) : 0;
                long LowAverage = _reports.Count > 0 ? (LowSum / _reports.Count) : 0;
                long Average = _reports.Count > 0 ? (AvgSum / _reports.Count) : 0;
                long MaxAverage = _reports.Count > 0 ? (MaxSum / _reports.Count) : 0;

                new BConsole.MessageBuilder()
                    .AppendLine( Blue, "TOTAL:")
                    .Append("  Received: ")
                    .Append(ReceivedSum==SentSum?DarkGreen:DarkRed, ReceivedSum)
                    .Append("/")
                    .Append(DarkGreen, SentSum)
                    .Append(" (")
                    .Append(ReceiveRate==100m?DarkGreen:DarkRed, ReceiveRate)
                    .Append("%), Low Average: ")
                    .Append(LowAverage<=500?DarkGreen:LowAverage<=5000?DarkYellow:DarkRed, LowAverage)
                    .Append("(ms), Average: ")
                    .Append(Average<=500?DarkGreen:Average<=5000?DarkYellow:DarkRed, Average)
                    .Append("(ms), Max Average: ")
                    .Append(MaxAverage<=500?DarkGreen:MaxAverage<=5000?DarkYellow:DarkRed, MaxAverage)
                    .WriteLine("(ms)");

                BConsole.WriteLine();
            }

        }

    }
}
