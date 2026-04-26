using PvPterraUtils.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using TShockAPI;
using Terraria;

namespace PvPterraUtils.Utils
{

    public static class PvPTerraMisc
    {

        public static int ParseCoinString(string costString)
        {
            if (string.IsNullOrWhiteSpace(costString) || costString.ToLower() == "none") return 0;

            int amount = 0;
            string valuePart = costString.Substring(0, costString.Length - 1);
            char typePart = costString.ToLower()[costString.Length - 1];

            if (int.TryParse(valuePart, out amount))
            {
                switch (typePart)
                {
                    case 'c': return amount;              
                    case 's': return amount * 100;        
                    case 'g': return amount * 10000;      
                    case 'p': return amount * 1000000;     
                    default: return 0;
                }
            }
            return 0;
        }

        public static string CopperToIconTag(long totalCopper)
        {
            if (totalCopper <= 0) return "Nada";

            long plat = totalCopper / 1000000;
            totalCopper %= 1000000;
            long gold = totalCopper / 10000;
            totalCopper %= 10000;
            long silver = totalCopper / 100;
            long copper = totalCopper % 100;

            var tags = new List<string>();

            if (plat > 0) tags.Add($"[i/s{plat}:74]");
            if (gold > 0) tags.Add($"[i/s{gold}:73]");
            if (silver > 0) tags.Add($"[i/s{silver}:72]");
            if (copper > 0) tags.Add($"[i/s{copper}:71]");

            return string.Join(" ", tags);
        }

        public static void BroadcastToRegion(string regionName, string message, byte r = 255, byte g = 255, byte b = 255)
        {
            foreach (var pvpPlayer in PvPterraCore.PvPPlayers)
            {
                if (pvpPlayer != null && pvpPlayer.CurrentRegion == regionName && pvpPlayer.TSPlayer != null)
                {
                    pvpPlayer.TSPlayer.SendMessage(message, r, g, b);
                }
            }
        }

        public static void SyncPvPState(TSPlayer player, bool isHostile, string teamName)
        {
            if (player == null || !player.Active) return;

            player.TPlayer.hostile = isHostile;
            NetMessage.SendData((int)PacketTypes.TogglePvp, -1, -1, null, player.Index);

            int teamId = 0; 
            if (!string.IsNullOrEmpty(teamName))
            {
                switch (teamName.ToLower())
                {
                    case "red": teamId = 1; break;
                    case "green": teamId = 2; break;
                    case "blue": teamId = 3; break;
                    case "yellow": teamId = 4; break;
                    case "pink": teamId = 5; break;
                }
            }

            player.TPlayer.team = teamId;
            NetMessage.SendData((int)PacketTypes.PlayerTeam, -1, -1, null, player.Index);
        }

        public static int GetTeamIdFromName(string teamName)
        {
            switch (teamName?.ToLower())
            {
                case "red": return 1;
                case "green": return 2;
                case "blue": return 3;
                case "yellow": return 4;
                case "pink": return 5;
                default: return 0; 
            }
        }
    }
}