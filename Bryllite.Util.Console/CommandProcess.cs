using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Bryllite.Util.Consoles
{
    public class CommandProcess
    {

        private CancellationTokenSource _cts;

        public string Prompt = "console:/>";

        public Action<string, string[]> OnCommand;


        public CommandProcess()
        {
        }

        public void Start( CancellationTokenSource cts = default(CancellationTokenSource) )
        {
            _cts = cts == default(CancellationTokenSource) ? new CancellationTokenSource() : cts;

            Task.Factory.StartNew(() =>
            {
                OnProcessUserCommand( _cts );
            });

            //Console.CancelKeyPress += (sender, e) =>
            //{
            //    e.Cancel = true;
            //    _cts?.Cancel();
            //};
        }

        public void Stop()
        {
            _cts?.Cancel();
        }

        private void OnProcessUserCommand(CancellationTokenSource cts)
        {
            while (!_cts.IsCancellationRequested)
            {
                try
                {
                    Console.Write(Prompt);
                    string[] tokens = Console.ReadLine().Trim().Split(' ');
                    if (tokens.Length > 0 && tokens[0].Length > 0)
                        OnCommand?.Invoke(tokens[0], tokens.Skip(1).ToArray());
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Console.CommandProcess() exception! e={e.Message}");
                    return;
                }
            }
        }
    }
}
