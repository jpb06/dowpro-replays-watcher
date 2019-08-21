using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration.Install;
using System.Linq;
using System.ServiceProcess;
using System.Threading.Tasks;

namespace DoWproReplayWatcher.Service
{
    [RunInstaller(true)]
    public partial class ProjectInstaller : Installer
    {
        public ProjectInstaller()
        {
            InitializeComponent();

            this.AfterInstall += new InstallEventHandler(ServiceInstaller_AfterInstall);
        }

        void ServiceInstaller_AfterInstall(object sender, InstallEventArgs e)
        {
            using (ServiceController sc = new ServiceController(DoWproWatcherServiceInstaller.ServiceName))
            {
                sc.Start();
            }
        }
    }
}
