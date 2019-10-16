using DoWproReplayWatcher.Logic.Application;
using DoWproReplayWatcher.Logic.Communication;
using DoWproReplayWatcher.Logic.Helpers;
using System;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;

namespace DoWproReplayWatcher.ManualTesting
{
    class Program
    {
        static async Task Main(string[] args)
        {
            FileHelper.CreateStructure();
            DoWproLadderApi.ApiUrl = "https://dowpro.cf/api";
            
            Logger logger = new Logger();
            await MainLogic.CheckPlayback(logger);
        }
    }
}