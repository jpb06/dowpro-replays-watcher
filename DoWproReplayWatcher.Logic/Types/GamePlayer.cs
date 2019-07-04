using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DoWproReplayWatcher.Logic.Types
{
    public class GamePlayer
    {
        public string Race { get; set; }
        public bool IsHuman { get; set; }
        public bool IsAmongWinners { get; set; }
        public int Team { get; set; }
        public string Name { get; set; }
        public int PTtlSc { get; set; }
    }
}
