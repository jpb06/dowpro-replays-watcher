using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DoWproReplayWatcher.Logic.Helpers
{
    public class RegistryHelper
    {
        public static string GetSoulstormInstallPath()
        {
            try
            {
                RegistryKey key = Registry.LocalMachine.OpenSubKey("SOFTWARE\\WOW6432Node\\THQ\\Dawn of War - Soulstorm");
                if (key != null)
                {
                    Object o = key.GetValue("InstallLocation");
                    if (o != null)
                        return o.ToString();
                }

                return null;
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
