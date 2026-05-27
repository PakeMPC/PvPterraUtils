using System;
using TShockAPI;
using Terraria;

namespace PvPterraUtils.Models
{
    public class PvPPlayer
    {
        public int Index { get; set; }
        public TSPlayer TSPlayer => TShock.Players[Index];

        public bool IsInPvP { get; set; }
        public string CurrentRegion { get; set; }
        public string Team { get; set; } 
        public string DefaultTeamPreference { get; set; } 
        public DateTime EntryTime { get; set; } = DateTime.Now;
        public bool IsWaitingForMatch { get; set; } = false;
        public bool IsSpectator { get; set; } = false;

        public DateTime LastCombatTime { get; set; }
        public string LastAttackerName { get; set; }

        public DateTime LastItemRestoreTime { get; set; } = DateTime.Now;
        public DateTime LastPotionRestoreTime { get; set; } = DateTime.Now;

        public int OriginalStatLifeMax { get; set; }
        public int OriginalStatManaMax { get; set; }

        public int PendingCoinReward { get; set; } = 0;

        public bool InCombat => (DateTime.Now - LastCombatTime).TotalSeconds < 15;

        public NetItem[] OriginalInventory { get; set; }
        public bool HasPaidEscrow { get; set; } 

        public string PreferredTeam { get; set; } = "none";

        public bool NeedsInventoryRestore { get; set; } = false;

        public bool PendingRespawnTeleport { get; set; } = false;
        public DateTime IgnoreRegionChangesUntil { get; set; } = DateTime.MinValue;

        public int KillStreak { get; set; } = 0;

        public PvPPlayer(int index)
        {
            Index = index;
            ResetPvPState();
            DefaultTeamPreference = "None";

            OriginalInventory = new NetItem[NetItem.MaxInventory];
        }

        public void ResetPvPState()
        {
            IsInPvP = false;
            CurrentRegion = string.Empty;
            Team = "None";
            HasPaidEscrow = false;
            LastAttackerName = string.Empty;
            LastCombatTime = DateTime.MinValue;
            KillStreak = 0;

            IsWaitingForMatch = true;
            IsSpectator = false;
            PendingRespawnTeleport = false;
        }
    }
}