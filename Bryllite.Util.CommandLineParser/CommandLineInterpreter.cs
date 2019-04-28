using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Bryllite.Util.Log;

namespace Bryllite.Util.CommandLine
{
    public class CommandLineInterpreter
    {
        public string Prompt = ">";
        public ConsoleColor PromptColor = ConsoleColor.DarkGray;

        public Action<string, string[]> OnCommand;

        private CancellationTokenSource cts;

        public CommandLineInterpreter()
        {
        }

        public void Start(CancellationTokenSource cts)
        {
            this.cts = cts;

            Task.Factory.StartNew(() =>
            {
                Run();
            });
        }

        public void Stop()
        {
            cts?.Cancel();
        }

        private static readonly char[] seperators = { ' ', '(', ')', '=', ',' };

        private void Run()
        {
            BConsole.WriteLine();
            BConsole.WriteLine(ConsoleColor.White, "Welcome to the Bryllite console!");

            while (!cts.IsCancellationRequested)
            {
                ShowPrompt();

                try
                {
                    string[] tokens = Console.ReadLine().Trim().Split(seperators);
                    if (tokens.Length == 0) continue;

                    string command = tokens[0];
                    if (command.Length == 0) continue;

                    List<string> args = new List<string>();
                    foreach (var arg in tokens.Skip(1).ToArray())
                        if (arg.Length > 0) args.Add(arg);

                    OnCommand?.Invoke(command, args.ToArray());
                }
                catch (Exception e)
                {
                    BConsole.WriteLine(ConsoleColor.Red, $"console exception! e={e.Message}");
                }
            }
        }

        public void ShowPrompt()
        {
            BConsole.Write(PromptColor, Prompt);
        }

    }
}
