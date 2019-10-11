using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DoWproReplayWatcher.Logic.Application
{
    public interface ILogger
    {
        void WriteEntry(string message);
    }
}
