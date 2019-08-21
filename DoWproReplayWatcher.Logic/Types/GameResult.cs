using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DoWproReplayWatcher.Logic.Types
{
    public class GameResult
    {
        public int PlayersCount { get; set; }
        public string WinCondition { get; set; }
        public int TeamsCount { get; set; }
        public int Duration { get; set; }
        public string MapName { get; set; }
        public List<GamePlayer> Players { get; set; }
        public string ModName { get; set; }
        public string ModVersion { get; set; }

        public GameResult()
        {
            this.Players = new List<GamePlayer>();
        }
    }
}
