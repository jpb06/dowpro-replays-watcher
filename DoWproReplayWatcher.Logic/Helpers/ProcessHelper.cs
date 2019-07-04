using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DoWproReplayWatcher.Logic.Helpers
{
    public class ProcessHelper
    {
        public static bool IsProcessRunning(string name)
        {
            Process[] pname = Process.GetProcessesByName(name);

            return pname.Length == 0 ? false : true;
        }
    }
}
