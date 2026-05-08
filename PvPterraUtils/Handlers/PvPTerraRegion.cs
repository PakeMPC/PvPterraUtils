using PvPterraUtils.Data;
using PvPterraUtils.Models;
using PvPterraUtils.utils;
using PvPterraUtils.Utils; 
using System;
using System.Linq;
using System.Threading.Tasks;
using Terraria;
using TerrariaApi.Server;
using TShockAPI;

namespace PvPterraUtils.Handlers
{
    public class PvPTerraRegion
    {
        private static DateTime _lastCheck = DateTime.Now;

        public static void OnGameUpdate(EventArgs args)
        {
            if ((DateTime.Now - _lastCheck).TotalMilliseconds < 500)
                return;

            _lastCheck = DateTime.Now;

            foreach (var pvpPlayer in PvPterraCore.PvPPlayers)
            {
                if (pvpPlayer == null || pvpPlayer.TSPlayer == null || !pvpPlayer.TSPlayer.Active)
                    continue;

                if (pvpPlayer.NeedsInventoryRestore && !pvpPlayer.TSPlayer.TPlayer.dead)
                {
                    pvpPlayer.NeedsInventoryRestore = false;
                    PvPTerraInventory.RestoreOriginalInventory(pvpPlayer);
                    pvpPlayer.TSPlayer.SendSuccessMessage(PvPTerrai18n.GetString("Reg_RespawnRestore"));
                }

                if (pvpPlayer.IsSpectator && !pvpPlayer.TSPlayer.TPlayer.dead && pvpPlayer.IsInPvP)
                {
                    if (pvpPlayer.TSPlayer.TPlayer.inventory[0].type != 5644)
                    {
                        PvPTerraInventory.ApplySpectatorInventory(pvpPlayer);
                        pvpPlayer.TSPlayer.SendErrorMessage(PvPTerrai18n.GetString("Reg_SpectatorEliminated"));
                    }
                }

                if (pvpPlayer.PendingRespawnTeleport && !pvpPlayer.TSPlayer.TPlayer.dead)
                {
                    pvpPlayer.PendingRespawnTeleport = false;

                    bool isMatchActive = PvPterraCore.PvPPlayers.Any(p => p != null && p.IsInPvP && p.CurrentRegion == pvpPlayer.CurrentRegion && !p.IsSpectator && !p.IsWaitingForMatch);

                    if (isMatchActive && !string.IsNullOrEmpty(pvpPlayer.CurrentRegion))
                    {
                        var region = TShock.Regions.GetRegionByName(pvpPlayer.CurrentRegion);
                        if (region != null)
                        {
                            pvpPlayer.TSPlayer.Teleport(region.Area.Center.X * 16, region.Area.Center.Y * 16);
                            pvpPlayer.IgnoreRegionChangesUntil = DateTime.Now.AddSeconds(3);
                        }
                    }
                    else
                    {
                        pvpPlayer.NeedsInventoryRestore = true;
                        pvpPlayer.IsInPvP = false;
                    }
                }

                if (pvpPlayer.IsInPvP)
                {
                    EnforceAssignedTeam(pvpPlayer);

                    if (!pvpPlayer.TSPlayer.TPlayer.dead)
                    {
                        if ((pvpPlayer.IsWaitingForMatch || pvpPlayer.IsSpectator) && pvpPlayer.TSPlayer.TPlayer.hostile)
                        {
                            pvpPlayer.TSPlayer.TPlayer.hostile = false;
                            NetMessage.SendData((int)PacketTypes.TogglePvp, -1, -1, null, pvpPlayer.TSPlayer.Index);
                        }
                        else if (!pvpPlayer.IsWaitingForMatch && !pvpPlayer.IsSpectator && !pvpPlayer.TSPlayer.TPlayer.hostile)
                        {
                            pvpPlayer.TSPlayer.TPlayer.hostile = true;
                            NetMessage.SendData((int)PacketTypes.TogglePvp, -1, -1, null, pvpPlayer.TSPlayer.Index);
                        }
                    }

                    if (!pvpPlayer.IsWaitingForMatch && !pvpPlayer.IsSpectator)
                    {
                        if (PvPTerraJson.Config.RegionConfigs.TryGetValue(pvpPlayer.CurrentRegion, out var regionConfig))
                        {
                            PvPTerraInventory.CheckAndRestock(pvpPlayer, regionConfig);
                        }
                    }
                }

                var tsPlayer = pvpPlayer.TSPlayer;

                if (pvpPlayer.PendingRespawnTeleport && !pvpPlayer.TSPlayer.TPlayer.dead)
                {
                    pvpPlayer.PendingRespawnTeleport = false;

                    bool isMatchActive = PvPterraCore.PvPPlayers.Any(p =>
                        p != null && p.IsInPvP && p.CurrentRegion == pvpPlayer.CurrentRegion &&
                        !p.IsSpectator && !p.IsWaitingForMatch);

                    if (isMatchActive && !string.IsNullOrEmpty(pvpPlayer.CurrentRegion))
                    {
                        var region = TShock.Regions.GetRegionByName(pvpPlayer.CurrentRegion);
                        if (region != null)
                        {
                            pvpPlayer.TSPlayer.Teleport(region.Area.Center.X * 16, region.Area.Center.Y * 16);
                            pvpPlayer.IgnoreRegionChangesUntil = DateTime.Now.AddSeconds(3);
                        }
                    }
                    else
                    {
                        pvpPlayer.NeedsInventoryRestore = true;
                        pvpPlayer.IsInPvP = false;
                        Utils.PvPTerraMisc.SyncPvPState(tsPlayer, false, "None");
                    }
                }

                if (tsPlayer.TPlayer.dead || DateTime.Now < pvpPlayer.IgnoreRegionChangesUntil)
                    continue;

                var regions = TShock.Regions.InAreaRegion(tsPlayer.TileX, tsPlayer.TileY);
                var currentTShockRegion = TShock.Regions.GetTopRegion(regions);
                string currentRegionName = currentTShockRegion != null ? currentTShockRegion.Name : string.Empty;

                if (pvpPlayer.CurrentRegion != currentRegionName)
                {
                    string oldRegion = pvpPlayer.CurrentRegion;
                    Data.PvPTerraJson.Config.RegionConfigs.TryGetValue(oldRegion, out var oldConfig);

                    if (!string.IsNullOrEmpty(currentRegionName))
                    {
                        EnterPvPRegion(pvpPlayer, currentRegionName);
                    }
                    else if (!string.IsNullOrEmpty(oldRegion))
                    {
                        bool fledInCombat = false;
                        bool isGlobalPvP = PvPTerraCommands.GlobalPvPActive;

                        bool usedPvPInv = false;
                        if (oldConfig != null)
                            usedPvPInv = oldConfig.PvPinventoryActive || PvPTerraJson.Config.PvPinventoryActive;

                        if (pvpPlayer.InCombat && !tsPlayer.TPlayer.dead && !isGlobalPvP)
                        {
                            var enemies = PvPterraCore.PvPPlayers.Where(p =>
                                p != null && p.IsInPvP && p.CurrentRegion == oldRegion &&
                                !p.IsSpectator && !p.IsWaitingForMatch && !p.TSPlayer.TPlayer.dead &&
                                p.TSPlayer.Index != tsPlayer.Index).ToList();

                            bool enemiesAlive = (oldConfig != null && !oldConfig.Mode.Equals("FFA", StringComparison.OrdinalIgnoreCase))
                                ? enemies.Any(p => p.Team != pvpPlayer.Team)
                                : enemies.Any();

                            if (enemiesAlive)
                            {
                                tsPlayer.DamagePlayer(9999);
                                tsPlayer.SendErrorMessage(PvPTerrai18n.GetString("Reg_FleeKill"));
                                fledInCombat = true;
                            }
                        }

                        pvpPlayer.ResetPvPState();
                        Utils.PvPTerraMisc.SyncPvPState(tsPlayer, false, "None");

                        if (tsPlayer.TPlayer.dead || fledInCombat)
                        {
                            if (usedPvPInv) pvpPlayer.NeedsInventoryRestore = true;
                        }
                        else
                        {
                            if (usedPvPInv)
                            {
                                PvPTerraInventory.RestoreOriginalInventory(pvpPlayer);
                                tsPlayer.SendSuccessMessage(PvPTerrai18n.GetString("Reg_LeaveRestore", oldRegion));
                            }
                            else
                            {
                                tsPlayer.SendSuccessMessage(PvPTerrai18n.GetString("Reg_LeaveNoRestore", oldRegion));
                            }
                        }

                        CheckMatchWinner(oldRegion);

                        if (PvPTerraJson.Config.CleanBuffsOnEnter)
                        {
                            for (int i = 0; i < Player.maxBuffs; i++)
                            {
                                tsPlayer.TPlayer.buffType[i] = 0;
                                tsPlayer.TPlayer.buffTime[i] = 0;
                            }
                        }
                    }
                }
            }
        }

        private static int GetRequiredPlayers(string mode)
        {
            if (string.IsNullOrWhiteSpace(mode)) return 0;

            string m = mode.ToLower().Replace(" ", "");

            string separator = m.Contains("vs") ? "vs" : (m.Contains("v") ? "v" : "");

            if (string.IsNullOrEmpty(separator)) return 0;

            int total = 0;
            var parts = m.Split(new string[] { separator }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var part in parts)
            {
                if (int.TryParse(part, out int num)) total += num;
            }
            return total;
        }

        public static void UpdateMatchState(string regionName, RegionSettings config)
        {

            bool isFFA = config.Mode.Equals("FFA", StringComparison.OrdinalIgnoreCase);

            bool isMatchActive = PvPterraCore.PvPPlayers.Any(p => p != null && p.IsInPvP && p.CurrentRegion == regionName && !p.IsSpectator && !p.IsWaitingForMatch);

            if (isMatchActive && !isFFA) return;

            int requiredPlayers = GetRequiredPlayers(config.Mode);

if (isFFA || requiredPlayers <= 0)
            {
                var allFFAPlayers = PvPterraCore.PvPPlayers
                    .Where(p => p != null && p.IsInPvP && p.CurrentRegion == regionName && !p.IsSpectator)
                    .ToList();
                var waitingFFAPlayers = allFFAPlayers.Where(p => p.IsWaitingForMatch).ToList();
                if (allFFAPlayers.Count < 2)
                {
                    foreach (var p in waitingFFAPlayers)
                    {
                        p.TSPlayer.SendInfoMessage(PvPTerrai18n.GetString("Reg_WaitingPlayers", config.Mode, allFFAPlayers.Count, 2));
                    }
                    return;
                }

                if (waitingFFAPlayers.Count > 0)
                {
                    bool anyoneKicked = false;
                    foreach (var p in waitingFFAPlayers)
                    {
                        if (!PvPTerraInventory.HasEnoughEscrow(p.TSPlayer, config.PvPreward))
                        {
                            p.TSPlayer.Teleport(Main.spawnTileX * 16, Main.spawnTileY * 16);
                            string costIcons = Utils.PvPTerraMisc.CopperToIconTag(Utils.PvPTerraMisc.ParseCoinString(config.PvPreward));
                            p.TSPlayer.SendErrorMessage(PvPTerrai18n.GetString("Reg_KickNoFunds", costIcons));
                            p.ResetPvPState();
                            Utils.PvPTerraMisc.SyncPvPState(p.TSPlayer, false, "None");
                            anyoneKicked = true;
                        }
                    }

                    if (anyoneKicked)
                    {
                        UpdateMatchState(regionName, config);
                        return;
                    }

                    foreach (var p in waitingFFAPlayers)
                    {
                        PvPTerraInventory.TryTakeEscrow(p.TSPlayer, config.PvPreward);
                        p.HasPaidEscrow = true;
                        p.IsWaitingForMatch = false; 
                        p.IsSpectator = false;

                        if (config.PvPinventoryActive || PvPTerraJson.Config.PvPinventoryActive)
                            PvPTerraInventory.ApplyPvPInventory(p.TSPlayer, config);

                        p.TSPlayer.TPlayer.hostile = true;
                        NetMessage.SendData((int)PacketTypes.TogglePvp, -1, -1, null, p.TSPlayer.Index);
                        p.LastItemRestoreTime = DateTime.Now;
                        p.LastPotionRestoreTime = DateTime.Now;
                        p.TSPlayer.SendSuccessMessage(PvPTerrai18n.GetString("Reg_FFAEnter"));
                    }
                }
                return;
            }

            var playersInRegion = PvPterraCore.PvPPlayers
                .Where(p => p != null && p.IsInPvP && p.CurrentRegion == regionName && !p.IsSpectator)
                .OrderBy(p => p.EntryTime)
                .ToList();

            int currentCount = playersInRegion.Count;

            if (currentCount < requiredPlayers)
            {
                foreach (var p in playersInRegion)
                {
                    if (!p.IsWaitingForMatch || p.TSPlayer.TPlayer.hostile)
                    {
                        p.TSPlayer.TPlayer.hostile = false;
                        NetMessage.SendData((int)PacketTypes.TogglePvp, -1, -1, null, p.TSPlayer.Index);
                    }

                    p.IsWaitingForMatch = true;
                    p.IsSpectator = false;
                    p.TSPlayer.SendInfoMessage(PvPTerrai18n.GetString("Reg_WaitingPlayers", config.Mode, currentCount, requiredPlayers));
                }
            }
            else
            {
                bool allHaveFunds = true;
                var playersToKick = new System.Collections.Generic.List<PvPPlayer>();

                for (int i = 0; i < requiredPlayers; i++)
                {
                    var p = playersInRegion[i];
                    if (!PvPTerraInventory.HasEnoughEscrow(p.TSPlayer, config.PvPreward))
                    {
                        allHaveFunds = false;
                        playersToKick.Add(p);
                    }
                }

                if (!allHaveFunds)
                {
                    foreach (var p in playersToKick)
                    {
                        p.TSPlayer.Teleport(Main.spawnTileX * 16, Main.spawnTileY * 16);

                        string costIcons = Utils.PvPTerraMisc.CopperToIconTag(Utils.PvPTerraMisc.ParseCoinString(config.PvPreward));
                        p.TSPlayer.SendErrorMessage(PvPTerrai18n.GetString("Reg_KickNoFunds", costIcons));

                        p.ResetPvPState();
                    }

                    var remainingPlayers = playersInRegion.Except(playersToKick).ToList();
                    foreach (var p in remainingPlayers)
                    {
                        p.TSPlayer.SendWarningMessage(PvPTerrai18n.GetString("Reg_MatchAbortedFunds"));
                    }

                    UpdateMatchState(regionName, config);
                    return; 
                }

                for (int i = 0; i < currentCount; i++)
                {
                    var p = playersInRegion[i];

                    if (i < requiredPlayers)
                    {
                        bool wasWaiting = p.IsWaitingForMatch;
                        p.IsWaitingForMatch = false;
                        p.IsSpectator = false;

                        if (wasWaiting)
                        {

                            if (!PvPTerraInventory.TryTakeEscrow(p.TSPlayer, config.PvPreward))
                            {
                                p.TSPlayer.Teleport(Main.spawnTileX * 16, Main.spawnTileY * 16);

                                string costIcons2 = Utils.PvPTerraMisc.CopperToIconTag(Utils.PvPTerraMisc.ParseCoinString(config.PvPreward));
                                p.TSPlayer.SendErrorMessage(PvPTerrai18n.GetString("Reg_KickNoFunds", costIcons2));

                                p.ResetPvPState();
                                continue; 
                            }

                            p.HasPaidEscrow = true;

                            if (config.PvPinventoryActive || PvPTerraJson.Config.PvPinventoryActive)
                            {
                                PvPTerraInventory.ApplyPvPInventory(p.TSPlayer, config);
                            }

                            p.TSPlayer.TPlayer.hostile = true;
                            NetMessage.SendData((int)PacketTypes.TogglePvp, -1, -1, null, p.TSPlayer.Index);

                            p.LastItemRestoreTime = DateTime.Now;
                            p.LastPotionRestoreTime = DateTime.Now;

                            p.TSPlayer.SendSuccessMessage(PvPTerrai18n.GetString("Reg_MatchStarted", requiredPlayers, config.Mode));
                        }
                    }
                }
            }
        }

        public static void CheckMatchWinner(string regionName)
        {
            if (!Data.PvPTerraJson.Config.RegionConfigs.TryGetValue(regionName, out var config)) return;

            if (config.Mode.Equals("FFA", StringComparison.OrdinalIgnoreCase)) return;

            var fighters = PvPterraCore.PvPPlayers
                .Where(p => p != null && p.IsInPvP && p.CurrentRegion == regionName && !p.IsWaitingForMatch && !p.IsSpectator && !p.TSPlayer.TPlayer.dead)
                .ToList();

            var activeTeams = fighters
                .Select(p => p.Team.ToLower() == "none" ? p.TSPlayer.Name : p.Team)
                .Distinct()
                .ToList();

            if ((activeTeams.Count <= 1 && fighters.Count > 0) || (fighters.Count == 0 && activeTeams.Count == 0))
            {
                string winnerTeam = activeTeams.Count == 1 ? activeTeams[0] : string.Empty;
                string winnerNames = "";

                if (activeTeams.Count == 1)
                {
                    var winningPlayersList = fighters.Where(p => (p.Team.ToLower() == "none" ? p.TSPlayer.Name : p.Team) == winnerTeam).ToList();
                    winnerNames = string.Join(", ", winningPlayersList.Select(p => p.TSPlayer.Name));

                    int wager = Utils.PvPTerraMisc.ParseCoinString(config.PvPreward);
                    foreach (var winner in winningPlayersList)
                    {
                        winner.PendingCoinReward += wager;
                    }
                }

                string announcement = activeTeams.Count == 1
                    ? PvPTerrai18n.GetString("Reg_MatchEndWinner", winnerTeam.ToUpper(), winnerNames)
                    : PvPTerrai18n.GetString("Reg_MatchEndTie");

                foreach (var p in PvPterraCore.PvPPlayers)
                {
                    if (p == null || p.TSPlayer == null || !p.TSPlayer.Active) continue;

                    bool isInRegion = p.CurrentRegion == regionName || p.IsSpectator;
                    bool foughtRecently = (DateTime.Now - p.LastCombatTime).TotalMinutes <= 5;

                    if (isInRegion || foughtRecently)
                    {
                        p.TSPlayer.SendMessage(announcement, 255, 215, 0);
                    }
                }

                Utils.PvPTerraMisc.BroadcastToRegion(regionName, PvPTerrai18n.GetString("Reg_ResetTimer"), 255, 255, 0);

                Task.Run(async () =>
                {
                    await Task.Delay(5000);

                    foreach (var p in PvPterraCore.PvPPlayers.Where(p => p != null && p.CurrentRegion == regionName))
                    {
                        if (p.TSPlayer.TPlayer.dead)
                        {
                            p.IsWaitingForMatch = false;
                            p.IsInPvP = false;
                            p.NeedsInventoryRestore = true; 
                            continue;
                        }

                        if (p.PreferredTeam.Equals("Spectator", StringComparison.OrdinalIgnoreCase))
                        {
                            p.IsSpectator = true;
                            p.IsWaitingForMatch = false; 

                            p.TSPlayer.TPlayer.hostile = false;
                            NetMessage.SendData((int)PacketTypes.TogglePvp, -1, -1, null, p.TSPlayer.Index);

                            continue; 
                        }

                        p.IsSpectator = false;
                        p.IsWaitingForMatch = true;

                        p.TSPlayer.TPlayer.hostile = false;
                        NetMessage.SendData((int)PacketTypes.TogglePvp, -1, -1, null, p.TSPlayer.Index);

                        if (p.TSPlayer.TPlayer.dead) p.NeedsInventoryRestore = true;
                        else PvPTerraInventory.RestoreOriginalInventory(p);
                    }

                    UpdateMatchState(regionName, config);
                });
            }
        }

        private static void EnforceAssignedTeam(PvPPlayer pvpPlayer)
        {
            int assignedTeam = Utils.PvPTerraMisc.GetTeamIdFromName(pvpPlayer.Team);

            if (pvpPlayer.TSPlayer.TPlayer.team != assignedTeam)
            {
                pvpPlayer.TSPlayer.TPlayer.team = assignedTeam;

                NetMessage.SendData((int)PacketTypes.PlayerTeam, -1, -1, null, pvpPlayer.TSPlayer.Index);

                pvpPlayer.TSPlayer.SendErrorMessage(PvPTerrai18n.GetString("Reg_TeamRestored"));
            }
        }


        private static void EnterPvPRegion(PvPPlayer pvpPlayer, string regionName)
        {
            try
            {
                if (PvPTerraJson.Config == null || !PvPTerraJson.Config.RegionConfigs.ContainsKey(regionName))
                    return;

                var regionConfig = PvPTerraJson.Config.RegionConfigs[regionName];
                var tsPlayer = pvpPlayer?.TSPlayer;

                if (regionConfig == null) return;
                if (tsPlayer == null || tsPlayer.TPlayer == null || !tsPlayer.Active) return;

                if (regionConfig.Mode == null) regionConfig.Mode = "FFA";
                if (regionConfig.PvPreward == null) regionConfig.PvPreward = "none";
                if (regionConfig.PvPbuffs == null) regionConfig.PvPbuffs = new System.Collections.Generic.List<int>();

                bool isFFA = regionConfig.Mode.Equals("FFA", StringComparison.OrdinalIgnoreCase);
                bool isMatchActive = !isFFA && PvPterraCore.PvPPlayers.Any(p => p != null && p.IsInPvP && p.CurrentRegion == regionName && !p.IsSpectator && !p.IsWaitingForMatch);

                if (isMatchActive || pvpPlayer.PreferredTeam.Equals("Spectator", StringComparison.OrdinalIgnoreCase))
                {
                    pvpPlayer.CurrentRegion = regionName;
                    pvpPlayer.IsInPvP = true;
                    pvpPlayer.IsSpectator = true;

                    pvpPlayer.IsWaitingForMatch = false;

                    pvpPlayer.Team = "None";

                    Utils.PvPTerraMisc.SyncPvPState(tsPlayer, false, "None");
                    PvPTerraInventory.ApplySpectatorInventory(pvpPlayer);

                    if (isMatchActive)
                        tsPlayer.SendErrorMessage(PvPTerrai18n.GetString("Reg_SpectatorInCourse"));
                    else
                        tsPlayer.SendInfoMessage(PvPTerrai18n.GetString("Reg_SpectatorPref"));

                    return;
                }

                if (!PvPterraCore.AssignPlayerTeam(pvpPlayer, regionConfig.Mode, regionName))
                {
                    tsPlayer.Teleport(Main.spawnTileX * 16, Main.spawnTileY * 16);

                    if (!string.IsNullOrEmpty(pvpPlayer.PreferredTeam) && pvpPlayer.PreferredTeam.ToLower() != "none")
                        tsPlayer.SendErrorMessage(PvPTerrai18n.GetString("Reg_TeamFull", pvpPlayer.PreferredTeam.ToUpper()));
                    else
                        tsPlayer.SendErrorMessage(PvPTerrai18n.GetString("Reg_ArenaFull", regionConfig.Mode));

                    return;
                }

                pvpPlayer.CurrentRegion = regionName;
                pvpPlayer.IsInPvP = true;

                pvpPlayer.LastItemRestoreTime = DateTime.Now;
                pvpPlayer.LastPotionRestoreTime = DateTime.Now;

                if (tsPlayer.TPlayer.CurrentLoadoutIndex != 0)
                {
                    tsPlayer.TPlayer.CurrentLoadoutIndex = 0;
                    NetMessage.SendData(147, -1, -1, null, tsPlayer.Index);
                }

                Utils.PvPTerraMisc.SyncPvPState(tsPlayer, false, pvpPlayer.Team);

                EnforcePvPBuffs(pvpPlayer, forceUpdate: true);

                tsPlayer.SendInfoMessage(PvPTerrai18n.GetString("PvP_EnterZone", regionName, regionConfig.Mode));

                pvpPlayer.EntryTime = DateTime.Now;
                UpdateMatchState(regionName, regionConfig);
            }
            catch (Exception ex)
            {
                TShock.Log.ConsoleError(PvPTerrai18n.GetString("Log_EnterRegionError", ex.Message, ex.StackTrace));

                pvpPlayer?.TSPlayer?.SendErrorMessage(PvPTerrai18n.GetString("Reg_InternalError"));
            }
        }

        private static void EnforcePvPBuffs(PvPPlayer pvpPlayer, bool forceUpdate = false)
        {
            try
            {
                if (string.IsNullOrEmpty(pvpPlayer.CurrentRegion)) return;

                int assignedTeam = Utils.PvPTerraMisc.GetTeamIdFromName(pvpPlayer.Team);

                if (pvpPlayer.TSPlayer.TPlayer.team != assignedTeam)
                {
                    pvpPlayer.TSPlayer.TPlayer.team = assignedTeam;
                    NetMessage.SendData((int)PacketTypes.PlayerTeam, -1, -1, null, pvpPlayer.TSPlayer.Index);

                    pvpPlayer.TSPlayer.SendErrorMessage(PvPTerrai18n.GetString("Reg_TeamRestored"));
                }


                if (!PvPTerraJson.Config.RegionConfigs.TryGetValue(pvpPlayer.CurrentRegion, out var regionConfig) || regionConfig == null)
                    return;

                if (regionConfig.PvPbuffs == null) return; 

                bool reapplied = false;
                foreach (int buffId in regionConfig.PvPbuffs)
                {
                    if (pvpPlayer.TSPlayer.TPlayer.FindBuffIndex(buffId) == -1)
                    {
                        pvpPlayer.TSPlayer.SetBuff(buffId, 3600 * 60); 
                        reapplied = true;
                    }
                }

                if (reapplied && !forceUpdate)
                {
                    pvpPlayer.TSPlayer.SendWarningMessage(PvPTerrai18n.GetString("Reg_StrictBuffs"));
                }
            }
            catch (Exception ex)
            {
                TShock.Log.ConsoleError(PvPTerrai18n.GetString("Log_BuffError", ex.Message));
            }
        }
    }
}
