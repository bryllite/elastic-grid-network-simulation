using System;

namespace NodeServerApp
{
    class Program
    {
        static void Main(string[] args)
        {
            // start with ServiceManager
            new ServiceManager().Run(args);
        }
    }
}
