using BrylliteLib.Crypto;
using BrylliteLib.Utils;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading;

namespace NodeServerApp
{
    public class ServiceManager
    {
        public static readonly string TAG = "ServiceManager";
        public static readonly string APP_CONFIG = "AppConfig.json";

        public AppConfig app_config = new AppConfig();

        public ServiceManager()
        {
        }

        private void Usage()
        {
            Log.WriteLine($"Usage: {BinFileName} {{host}} {{sPort:ePort}}");
            Log.WriteLine($"ex1) {BinFileName} 192.168.0.x 19000");
            Log.WriteLine($"ex2) {BinFileName} 192.168.0.x 19000:19010");
        }

        // 서비스 시작
        public void Run(string[] args)
        {
            if (args.Length != 2)
            {
                Log.i(TAG, $"Invalid arguments!");
                Usage();
                return;
            }

            string host = args[0];
            string[] ports = args[1].Split(':');
            int sPort = Convert.ToInt32(ports[0]);
            int ePort = ports.Length == 2 ? Convert.ToInt32(ports[1]) : sPort;

            try
            {
                // 설정 정보 읽어오기
                string json = File.ReadAllText(APP_CONFIG);
                app_config = JsonConvert.DeserializeObject<AppConfig>(json);
            }
            catch (Exception e)
            {
                Log.w(TAG, $"Loading {APP_CONFIG} Exception! e={e.Message}");
                return;
            }

            if (sPort != ePort)
            {
                RunInstance(host, sPort, ePort, app_config.Redirect);
            }
            else
            {
                Run(host, sPort);
            }
        }

        public void Run(string host, int port)
        {
            // 노드 키
            // (노트: 원래는 해당 노드의 저장된 키를 로드해서 사용해야 한다)
            CPrivateKey NodeKey = CPrivateKey.CreateKey();

            // 로그 파일 명
            //            Log.SetLogFilePath( $"logs/{NodeKey.Address}.log" );

            // 노드 서버 생성
            NodeServer nodeServer = new NodeServer(NodeKey);

            // 콘솔 캔슬 키
            Console.CancelKeyPress += (sender, e) =>
            {
                Log.i($"CTRL+C key pressed!");
                e.Cancel = true;
                nodeServer.Stop();
            };

            // 노드 서버 개시
            nodeServer.Start(host, port, app_config.TrackerHost, app_config.TrackerPort);
            while (nodeServer.Running)
            {
                Thread.Sleep(10);

                // 노드 서버 업데이트
                nodeServer.Update();
            }

            Log.i(TAG, $"NodeServerApp Terminated!");
        }

        // 현재 실행파일 경로
        private static string BinPath
        {
            get
            {
                return Assembly.GetEntryAssembly().Location;
            }
        }

        private static string BinFileName
        {
            get
            {
                return Path.GetFileName(Assembly.GetEntryAssembly().Location);
            }
        }

        // 범위 실행인 경우 프로세스 목록
        private List<Process> _processes = new List<Process>();

        private void RunInstance(string host, int sPort, int ePort, bool redirect)
        {
            for (int i = sPort; i <= ePort; i++)
            {
                Thread.Sleep(5);

                Process p = RunInstance(host, i, redirect);
                _processes.Add(p);
            }

            // 캔슬키 하위 프로세스 모두 삭제
            Console.CancelKeyPress += (sender, e) =>
            {
                e.Cancel = true;
                foreach (Process p in _processes)
                    p.Kill();
            };

            // 모든 프로세스 종료 대기한다.
            foreach (Process p in _processes)
                p.WaitForExit();
        }

        private Process RunInstance(string host, int port, bool redirect)
        {
            Process process = new Process();
            process.StartInfo.FileName = BinFileName;
            process.StartInfo.Arguments = $"{host} {port.ToString()}";
            process.StartInfo.CreateNoWindow = true;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardOutput = redirect;
            process.StartInfo.RedirectStandardError = redirect;

            process.Start();

            if (redirect)
            {
                process.OutputDataReceived += OnOutputDataReceived;
                process.ErrorDataReceived += OnErrorDataReceived;
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();
            }

            return process;
        }

        private void OnOutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            Log.d(e.Data);
        }

        private void OnErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            Log.d(e.Data);
        }
    }
}
