using DoWproReplayWatcher.Logic;
using DoWproReplayWatcher.Logic.Communication;
using DoWproReplayWatcher.Logic.Helpers;
using DoWproReplayWatcher.Logic.Types;
using DoWproReplayWatcher.Lua;
using DoWproReplayWatcher.RelicChunky;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.ServiceProcess;
using System.Threading;
using System.Configuration;
using DoWproReplayWatcher.Logic.Application;

namespace DoWproReplayWatcher.Service
{
    public partial class DoWproWatcherService : ServiceBase
    {
        private Thread loopThread;

        public DoWproWatcherService()
        {
            InitializeComponent();

            this.eventLog = new EventLog();
            if (!EventLog.SourceExists("DoWproWatcherService"))
                EventLog.CreateEventSource("DoWproWatcherService", "Application");

            this.eventLog.Source = "DoWproWatcherService";
            this.eventLog.Log = "Application";

            this.loopThread = null;
        }

        protected override void OnStart(string[] args)
        {
            DoWproLadderApi.ApiUrl = ConfigurationManager.AppSettings["ApiUrl"];
            FileHelper.CreateStructure();

            Logger logger = new Logger(this.eventLog);
            this.loopThread = new Thread(async () => await MainLogic.CheckPlayback(logger));
            this.loopThread.Start();
        }

        protected override void OnStop()
        {
            this.loopThread.Abort();
        }
    }
}