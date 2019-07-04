using DoWproReplayWatcher.Logic.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DoWproReplayWatcher.Logic
{
    public class Constants
    {
        public static string SoulstormInstallPath = RegistryHelper.GetSoulstormInstallPath();

        public static string PlayBackPath = Path.Combine(SoulstormInstallPath, "Playback");
        public static string TempRecPath = Path.Combine(SoulstormInstallPath, "Playback", "temp.rec");
    }
}
