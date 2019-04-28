using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace Bryllite.Util.CommandLine
{
    public class CommandLineParser : CommandLineBuilder
    {
        public CommandLineParser() : base()
        {
        }

        public CommandLineParser(string[] args) : this()
        {
            Parse(args);
        }

        public CommandLineParser(CommandLineParser other)
        {
            Parse(other.ToStringArray());
        }

        protected void Parse(string[] args)
        {
            foreach (var arg in args)
            {
                string[] tokens = arg.Split('=');

                string key = tokens[0].Trim(' ', '-' );
                string value = tokens.Length > 1 ? tokens[1].Trim() : "true";

                KeyValuePairs.Add(key, value);
            }
        }
    }
}
