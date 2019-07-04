using DoWproReplayWatcher.Logic.Lua;
using DoWproReplayWatcher.Logic.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DoWproReplayWatcher.Logic.Extensions
{
    public static class LuaObjectExtensions
    {
        public static GameResult AsGameResult(this LuaObject root)
        {
            if (root == null) return null;

            LuaObject stats = root.GetValue<LuaObject>("GSGameStats");

            if (stats != null)
            {
                GamePlayer player1 = stats.GetValue<LuaObject>("player_0").AsGamePlayer();
                GamePlayer player2 = stats.GetValue<LuaObject>("player_1").AsGamePlayer();

                if (stats.Contains("Players") && stats.Is<int>("Players") &&
                    stats.Contains("WinBy") && stats.Is<string>("WinBy") &&
                    stats.Contains("Teams") && stats.Is<int>("Teams") &&
                    stats.Contains("Duration") && stats.Is<int>("Duration") &&
                    stats.Contains("Scenario") && stats.Is<string>("Scenario") &&
                    player1 != null && player2 != null)
                {
                    GameResult result = new GameResult
                    {
                        MapName = stats.GetValue<string>("Scenario"),
                        Duration = stats.GetValue<int>("Duration"),
                        PlayersCount = stats.GetValue<int>("Players"),
                        TeamsCount = stats.GetValue<int>("Teams"),
                        WinCondition = stats.GetValue<string>("WinBy"),
                        Players = new List<GamePlayer>() { player1, player2 }
                    };
                    return result;
                }
            }

            return null;
        }

        public static GamePlayer AsGamePlayer(this LuaObject root)
        {
            if (root != null &&
                root.Contains("PRace") && root.Is<string>("PRace") &&
                root.Contains("PHuman") && root.Is<int>("PHuman") &&
                root.Contains("PFnlState") && root.Is<int>("PFnlState") &&
                root.Contains("PTeam") && root.Is<int>("PTeam") &&
                root.Contains("PName") && root.Is<string>("PName") &&
                root.Contains("PTtlSc") && root.Is<int>("PTtlSc"))
            {
                GamePlayer player = new GamePlayer
                {
                    IsAmongWinners = root.GetValue<int>("PFnlState") == 5,
                    IsHuman = root.GetValue<int>("PHuman") == 1,
                    Name = root.GetValue<string>("PName"),
                    PTtlSc = root.GetValue<int>("PTtlSc"),
                    Race = root.GetValue<string>("PRace"), 
                    Team = root.GetValue<int>("PTeam")
                };
                return player;
            }

            return null;
        }
    }
}
