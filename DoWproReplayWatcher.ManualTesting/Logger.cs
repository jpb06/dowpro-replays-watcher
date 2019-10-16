using DoWproReplayWatcher.Logic.Application;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DoWproReplayWatcher.ManualTesting
{
    public class Logger : ILogger
    {
        public void WriteEntry(string message)
        {
            Console.WriteLine(message);
        }
    }
}
