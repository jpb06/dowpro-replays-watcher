using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DoWproReplayWatcher.Logic.Application
{
    public class Logger : ILogger
    {
        private EventLog eventLog;

        public Logger() { }
        public Logger(EventLog eventLog)
        {
            this.eventLog = eventLog;
        }

        public void WriteEntry(string message)
        {
            if (this.eventLog != null)
                this.eventLog.WriteEntry(message);
            else
                Console.WriteLine(message);
        }
    }
}
