using BrylliteLib.Crypto;
using BrylliteLib.Hash;
using BrylliteLib.Net;
using BrylliteLib.Net.Elastic;
using BrylliteLib.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TrackerServiceApp
{
    class Program
    {
        // tracker service
        private static TrackerService tracker = new TrackerService();

        // tracker port
        private static int Port = TCPHelper.TRACKER_PORT;
        private static readonly int TimeLimit = 60000;

        static void Main(string[] args)
        {
            try
            {
                Port = Convert.ToInt32(args[0]);
            }
            catch (Exception)
            {
                Log.d($"host port not specified! using default host:port");
            }

            Log.i($"TrackerServiceApp starting on port {Port}");

            // 실행시 자동 시작
            Start();

            // 헬프 메세지
            Help();

            // 콘솔 입력 처리
            Task t = Task.Factory.StartNew(() =>
            {
                RunConsoleCommand();
            });

            // 메인 루프
            while (!_exit)
            {
                Thread.Sleep(10);

                // 트래커 업데이트
                if (tracker.Running)
                    tracker.Update();
            }

            // wait for exit confirm
            Log.PressAnyKey();
        }

        private static bool _exit = false;
        static void RunConsoleCommand()
        {
            while (!_exit)
            {
                Log.Write("Command:/>");

                string line = Console.ReadLine();
                string command = line.Trim();
                if (string.IsNullOrEmpty(command)) continue;

                string[] commands = command.Split(' ');
                switch (commands[0])
                {
                    case "help": Help(); break;
                    case "exit": _exit = true; break;
                    case "start": Start(); break;
                    case "stop": Stop(); break;
                    case "run": Run(commands); break;
                    case "info": Info(); break;
                    case "report": Report(); break;
                    default: Log.d($"Unknown Command"); break;
                }
            }
        }

        static void Help()
        {
            Log.WriteLine("[TrackerServiceApp commands]");
            Log.WriteLine("help: show help message");
            Log.WriteLine("exit: exit the app");
            Log.WriteLine("start: start the TrackerService");
            Log.WriteLine("stop: stop the TrackerService");
            Log.WriteLine("run: run test. usage: run [block_size] [broadcast_times]");
            Log.WriteLine("ex) run 128 10 {{128kb, 10 times}}");
            Log.WriteLine("info: show information");
        }

        static void Info()
        {
            tracker.Information();
        }

        static void Report()
        {
            tracker.Report();
        }

        static void Start()
        {
            if ( !tracker.Running )
                tracker.Start(TCPHelper.IP_ANY, Port);
        }

        static void Stop()
        {
            tracker.Stop();
        }

        static void Run(string[] commands)
        {
            if (commands.Length != 3)
            {
                Log.i($"Run() failed! invalid arguments!");
                return;
            }

            try
            {
                int nBlockSize = 1024 * Convert.ToInt32(commands[1]);
                int nTimes = Convert.ToInt32(commands[2]);

                tracker.RunTest(nBlockSize, nTimes, TimeLimit);
            }
            catch (Exception e)
            {
                Log.w($"Run() FAILED! exception raised! e={e.Message}");
            }
        }
    }
}
