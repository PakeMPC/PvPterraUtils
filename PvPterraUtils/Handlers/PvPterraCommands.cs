using PvPterraUtils.Data;
using PvPterraUtils.Models;
using PvPterraUtils.utils;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using TShockAPI;

namespace PvPterraUtils.Handlers
{
    public class PvPTerraCommands
    {

        public static void InfoCommand(CommandArgs args)
        {
            var pvpPlayer = PvPterraCore.PvPPlayers[args.Player.Index];

            if (pvpPlayer == null || !pvpPlayer.IsInPvP)
            {
                args.Player.SendInfoMessage(PvPTerrai18n.GetString("Cmd_NotInPvP"));
                return;
            }

            if (!PvPTerraJson.Config.RegionConfigs.TryGetValue(pvpPlayer.CurrentRegion, out var regionConfig))
            {
                args.Player.SendErrorMessage(PvPTerrai18n.GetString("Cmd_Info_Error"));
                return;
            }

            args.Player.SendMessage(PvPTerrai18n.GetString("Cmd_Info_Title", pvpPlayer.CurrentRegion), 255, 165, 0);
            args.Player.SendMessage(PvPTerrai18n.GetString("Cmd_Info_Mode", regionConfig.Mode), 255, 255, 255);

            string infoRewardIcon = Utils.PvPTerraMisc.CopperToIconTag(Utils.PvPTerraMisc.ParseCoinString(regionConfig.PvPreward));
            args.Player.SendMessage(PvPTerrai18n.GetString("Cmd_Info_Reward", infoRewardIcon), 255, 215, 0);

            string teamText = pvpPlayer.Team == "None" ? PvPTerrai18n.GetString("Cmd_Info_TeamFFA") : pvpPlayer.Team;
            args.Player.SendMessage(PvPTerrai18n.GetString("Cmd_Info_Team", teamText), 173, 216, 230);

            if (pvpPlayer.InCombat)
            {
                int secondsLeft = 15 - (int)(DateTime.Now - pvpPlayer.LastCombatTime).TotalSeconds;
                args.Player.SendMessage(PvPTerrai18n.GetString("Cmd_Info_InCombat", secondsLeft), 255, 0, 0);
                args.Player.SendMessage(PvPTerrai18n.GetString("Cmd_Info_Attacker", pvpPlayer.LastAttackerName), 255, 100, 100);
            }
            else
            {
                args.Player.SendMessage(PvPTerrai18n.GetString("Cmd_Info_Safe"), 0, 255, 0);
            }
        }

        public static void RankingCommand(CommandArgs args)
        {
            string targetRegion = args.Parameters.Count > 0 ? string.Join(" ", args.Parameters) : "";

            var topPlayers = PvPterraCore.Database.GetTopPlayers(10, targetRegion);

            if (topPlayers.Count == 0)
            {
                args.Player.SendInfoMessage(PvPTerrai18n.GetString("Cmd_Rank_Empty"));
                return;
            }

            args.Player.SendMessage("---------------------------------------", 255, 255, 0);

            foreach (string line in topPlayers.AsEnumerable().Reverse())
            {
                args.Player.SendMessage(line, 255, 255, 255);
            }

            if (string.IsNullOrEmpty(targetRegion))
            {
                args.Player.SendMessage(PvPTerrai18n.GetString("Cmd_Rank_Global"), 255, 255, 0);
            }
            else
            {
                args.Player.SendMessage(PvPTerrai18n.GetString("Cmd_Rank_Region", targetRegion.ToUpper()), 255, 255, 0);
            }
        }

        public static void TeamCommand(CommandArgs args)
        {
            var pvpPlayer = PvPterraCore.PvPPlayers[args.Player.Index];

            if (args.Parameters.Count == 0)
            {
                args.Player.SendErrorMessage(PvPTerrai18n.GetString("Cmd_Team_Syntax"));
                args.Player.SendInfoMessage(PvPTerrai18n.GetString("Cmd_Team_Options"));
                return;
            }

            string chosenTeam = args.Parameters[0].ToLower();
            List<string> validTeams = new List<string> { "red", "blue", "green", "yellow", "pink", "none", "spectator" };

            if (!validTeams.Contains(chosenTeam))
            {
                args.Player.SendErrorMessage(PvPTerrai18n.GetString("Cmd_InvalidTeam"));
                return;
            }

            chosenTeam = char.ToUpper(chosenTeam[0]) + chosenTeam.Substring(1);
            pvpPlayer.PreferredTeam = chosenTeam;

            if (chosenTeam == "None")
                args.Player.SendSuccessMessage(PvPTerrai18n.GetString("Cmd_TeamCleared"));
            else
                args.Player.SendSuccessMessage(PvPTerrai18n.GetString("Cmd_TeamSet", chosenTeam));

            if (pvpPlayer.IsInPvP)
            {
                pvpPlayer.Team = chosenTeam;
                Utils.PvPTerraMisc.SyncPvPState(pvpPlayer.TSPlayer, true, chosenTeam);
            }
        }


        public static bool GlobalPvPActive = false;
        public static bool ForceTeamMode = false;
        public static bool IgnoreTeamMode = false;

        public static void ActiveCommand(CommandArgs args)
        {
            if (args.Parameters.Count < 1)
            {
                args.Player.SendErrorMessage(PvPTerrai18n.GetString("Cmd_Active_Syntax"));
                return;
            }

            GlobalPvPActive = args.Parameters[0].ToLower() == "t" || args.Parameters[0].ToLower() == "true";

            if (!GlobalPvPActive)
            {
                ForceTeamMode = false;
                IgnoreTeamMode = false;
                TShock.Utils.Broadcast(PvPTerrai18n.GetString("Cmd_Active_Off"), 255, 255, 255);

                foreach (var pvpPlayer in PvPterraCore.PvPPlayers)
                {
                    if (pvpPlayer != null && pvpPlayer.TSPlayer != null && pvpPlayer.TSPlayer.Active)
                    {
                        pvpPlayer.IsInPvP = false;
                        pvpPlayer.Team = "None";
                        Utils.PvPTerraMisc.SyncPvPState(pvpPlayer.TSPlayer, false, "None");
                    }
                }
                return;
            }

            IgnoreTeamMode = args.Parameters.Contains("--ignoreteam");
            ForceTeamMode = args.Parameters.Contains("--forceteam");

            string modo = args.Parameters.Count > 1 ? args.Parameters[1] : "FFA";
            string recompensa = args.Parameters.Count > 2 ? args.Parameters[2] : "none";

            TShock.Utils.Broadcast(PvPTerrai18n.GetString("Cmd_Active_On"), 255, 0, 0);

            string activeRewardIcon = Utils.PvPTerraMisc.CopperToIconTag(Utils.PvPTerraMisc.ParseCoinString(recompensa));
            TShock.Utils.Broadcast(PvPTerrai18n.GetString("Cmd_Active_Status", modo, activeRewardIcon), 255, 255, 0);

            foreach (var pvpPlayer in PvPterraCore.PvPPlayers)
            {
                if (pvpPlayer != null && pvpPlayer.TSPlayer != null && pvpPlayer.TSPlayer.Active)
                {
                    pvpPlayer.IsInPvP = true;
                    pvpPlayer.IsSpectator = false;
                    pvpPlayer.IsWaitingForMatch = false;

                    PvPterraCore.AssignPlayerTeam(pvpPlayer, modo, "");

                    Utils.PvPTerraMisc.SyncPvPState(pvpPlayer.TSPlayer, true, pvpPlayer.Team);
                    pvpPlayer.TSPlayer.SendWarningMessage(PvPTerrai18n.GetString("Cmd_Active_Force", pvpPlayer.Team));
                }
            }
        }

        public static void RegionCommand(CommandArgs args)
        {
            if (args.Parameters.Count == 0)
            {
                args.Player.SendErrorMessage(PvPTerrai18n.GetString("Cmd_Reg_Help_Exist"));
                args.Player.SendInfoMessage(PvPTerrai18n.GetString("Cmd_Reg_Help_Assign"));
                args.Player.SendInfoMessage(PvPTerrai18n.GetString("Cmd_Reg_Help_Remove"));
                args.Player.SendInfoMessage(PvPTerrai18n.GetString("Cmd_Reg_Help_Redefine"));

                args.Player.SendMessage("-----------------------------------------", 150, 150, 150);

                args.Player.SendErrorMessage(PvPTerrai18n.GetString("Cmd_Reg_Help_New"));
                args.Player.SendInfoMessage(PvPTerrai18n.GetString("Cmd_Reg_Help_Set"));
                args.Player.SendInfoMessage(PvPTerrai18n.GetString("Cmd_Reg_Help_Define"));
                args.Player.SendInfoMessage(PvPTerrai18n.GetString("Cmd_Reg_Help_Delete"));
                return;
            }

            string subCmd = args.Parameters[0].ToLower();

            switch (subCmd)
            {
                case "assign":
                    if (args.Parameters.Count < 3)
                    {
                        args.Player.SendErrorMessage(PvPTerrai18n.GetString("Cmd_Reg_Assign_Syntax"));
                        return;
                    }

                    string asignName = args.Parameters[1];
                    string asignMode = args.Parameters[2];
                    string asignReward = args.Parameters.Count >= 4 ? args.Parameters[3] : "none";

                    if (TShock.Regions.GetRegionByName(asignName) == null)
                    {
                        args.Player.SendErrorMessage(PvPTerrai18n.GetString("Cmd_Reg_NotExist", asignName));
                        return;
                    }

                    PvPTerraJson.Config.RegionConfigs[asignName] = new RegionSettings { Mode = asignMode, PvPreward = asignReward };
                    PvPTerraJson.SaveConfig();

                    args.Player.SendSuccessMessage(PvPTerrai18n.GetString("Cmd_Reg_Assigned", asignName, asignMode, asignReward));
                    break;

                case "define":
                    if (args.Parameters.Count < 3)
                    {
                        args.Player.SendErrorMessage(PvPTerrai18n.GetString("Cmd_Reg_Define_Syntax"));
                        return;
                    }

                    string defName = args.Parameters[1];
                    string defMode = args.Parameters[2];
                    string defReward = args.Parameters.Count >= 4 ? args.Parameters[3] : "none";

                    if (TShock.Regions.GetRegionByName(defName) == null)
                    {
                        Commands.HandleCommand(args.Player, $"/region define {defName}");
                        if (TShock.Regions.GetRegionByName(defName) == null) return;
                    }

                    PvPTerraJson.Config.RegionConfigs[defName] = new RegionSettings { Mode = defMode, PvPreward = defReward };
                    PvPTerraJson.SaveConfig();
                    args.Player.SendSuccessMessage(PvPTerrai18n.GetString("Cmd_Reg_Defined", defMode, defReward));
                    break;

                case "remove":
                    if (args.Parameters.Count < 2)
                    {
                        args.Player.SendErrorMessage(PvPTerrai18n.GetString("Cmd_Reg_Remove_Syntax"));
                        return;
                    }

                    string removeName = args.Parameters[1];

                    if (PvPTerraJson.Config.RegionConfigs.ContainsKey(removeName))
                    {
                        PvPTerraJson.Config.RegionConfigs.Remove(removeName);
                        PvPTerraJson.SaveConfig();
                        args.Player.SendSuccessMessage(PvPTerrai18n.GetString("Cmd_Reg_Removed", removeName));
                    }
                    else
                    {
                        args.Player.SendErrorMessage(PvPTerrai18n.GetString("Cmd_Reg_NotAssigned", removeName));
                    }
                    break;

                case "redefine":
                    if (args.Parameters.Count < 4)
                    {
                        args.Player.SendErrorMessage(PvPTerrai18n.GetString("Cmd_Reg_Redefine_Syntax"));
                        args.Player.SendInfoMessage(PvPTerrai18n.GetString("Cmd_Reg_Redefine_Example"));
                        return;
                    }

                    string redName = args.Parameters[1];
                    string redMode = args.Parameters[2];
                    string redReward = args.Parameters[3];

                    if (PvPTerraJson.Config.RegionConfigs.ContainsKey(redName))
                    {
                        PvPTerraJson.Config.RegionConfigs[redName].Mode = redMode;
                        PvPTerraJson.Config.RegionConfigs[redName].PvPreward = redReward;
                        PvPTerraJson.SaveConfig();

                        args.Player.SendSuccessMessage(PvPTerrai18n.GetString("Cmd_Reg_Redefined", redName, redMode, redReward));
                    }
                    else
                    {
                        args.Player.SendErrorMessage(PvPTerrai18n.GetString("Cmd_Reg_NotAssigned", redName));
                    }
                    break;

                case "set":
                    if (args.Parameters.Count > 1)
                        Commands.HandleCommand(args.Player, $"/region set {args.Parameters[1]}");
                    else
                        args.Player.SendErrorMessage(PvPTerrai18n.GetString("Cmd_Reg_Set_Syntax"));
                    break;

                case "delete":
                    if (args.Parameters.Count < 2)
                    {
                        args.Player.SendErrorMessage(PvPTerrai18n.GetString("Cmd_Reg_Delete_Syntax"));
                        return;
                    }

                    string delName = args.Parameters[1];

                    Commands.HandleCommand(args.Player, $"/region delete {delName}");

                    if (PvPTerraJson.Config.RegionConfigs.ContainsKey(delName))
                    {
                        PvPTerraJson.Config.RegionConfigs.Remove(delName);
                        PvPTerraJson.SaveConfig();
                    }
                    args.Player.SendSuccessMessage(PvPTerrai18n.GetString("Cmd_Reg_Deleted", delName));
                    break;

                default:
                    args.Player.SendErrorMessage(PvPTerrai18n.GetString("Cmd_Reg_Unknown"));
                    break;
            }
        }
    }
}