using PvPterraUtils.Data;
using PvPterraUtils.Handlers;
using PvPterraUtils.Models;
using PvPterraUtils.utils;
using PvPterraUtils.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Terraria;
using TerrariaApi.Server;
using TShockAPI;
using TShockAPI.Hooks;

namespace PvPterraUtils
{
    [ApiVersion(2, 1)]
    public class PvPterraCore : TerrariaPlugin
    {
        public override string Name => "PvPterraUtils";
        public override Version Version => new Version(1, 0, 7, 0);
        public override string Author => "PakeMPC";
        public override string Description => "Advanced PvP management system with multiple tools.";

        public static PvPTerraDatabase Database;
        public static PvPPlayer[] PvPPlayers;

        public PvPterraCore(Main game) : base(game)
        {
            PvPPlayers = new PvPPlayer[Main.maxPlayers];
        }

        public override void Initialize()

        {
            Data.PvPTerraJson.LoadConfig();

            Database = new PvPTerraDatabase();

            for (int i = 0; i < Main.maxPlayers; i++)
                PvPPlayers[i] = new PvPPlayer(i);

            ServerApi.Hooks.ServerLeave.Register(this, OnPlayerLeave);

            ServerApi.Hooks.GameUpdate.Register(this, Handlers.PvPTerraRegion.OnGameUpdate);
            ServerApi.Hooks.ServerJoin.Register(this, OnServerJoin);
            ServerApi.Hooks.NetGetData.Register(this, Handlers.PvPTerraCombat.OnGetData, int.MinValue);

            GeneralHooks.ReloadEvent += OnReload;
            TShockAPI.Hooks.PlayerHooks.PlayerPostLogin += OnPlayerLogin;

            RegisterCommands();
        }

        private void RegisterCommands()
        {
            Commands.ChatCommands.Add(new Command(new List<string> { "pvpterra.team" }, TeamCommand, "pvpteam"));
            Commands.ChatCommands.Add(new Command(new List<string> { "pvpterra.ranking" }, RankingCommand, "pvpranking"));
            Commands.ChatCommands.Add(new Command(new List<string> { "pvpterra.user" }, InfoCommand, "pvp"));

            Commands.ChatCommands.Add(new Command(new List<string> { "pvpterra.admin" }, RegionCommand, "pvpregion"));
            Commands.ChatCommands.Add(new Command(new List<string> { "pvpterra.admin" }, ActiveCommand, "pvpactive"));
        }

        private void OnPlayerLeave(LeaveEventArgs args)
        {
            var pvpPlayer = PvPPlayers[args.Who];
            if (pvpPlayer != null && pvpPlayer.IsInPvP && !pvpPlayer.IsSpectator && !pvpPlayer.IsWaitingForMatch)
            {
                int tagDuration = Data.PvPTerraJson.Config.CombatTagDurationSeconds;
                if ((DateTime.Now - pvpPlayer.LastCombatTime).TotalSeconds <= tagDuration && !string.IsNullOrEmpty(pvpPlayer.LastAttackerName))
                {
                    var killer = TShock.Players.FirstOrDefault(p => p != null && p.Active && p.Name == pvpPlayer.LastAttackerName);
                    if (killer != null)
                    {
                        Handlers.PvPTerraCombat.GiveKillReward(killer, pvpPlayer);
                        TShock.Log.ConsoleInfo(PvPTerrai18n.GetString("Log_LeaveCombat", pvpPlayer.TSPlayer.Name, killer.Name));
                        killer.SendSuccessMessage(PvPTerrai18n.GetString("Core_FleeReward", pvpPlayer.TSPlayer.Name));
                    }
                }

                pvpPlayer.IsInPvP = false;

                Handlers.PvPTerraRegion.CheckMatchWinner(pvpPlayer.CurrentRegion);
            }
        }

        private void OnReload(ReloadEventArgs e)
        {
            Data.PvPTerraJson.LoadConfig();

            e.Player.SendSuccessMessage(PvPTerrai18n.GetString("Core_ReloadSuccess"));
        }

        private void OnServerJoin(JoinEventArgs args)
        {
            PvPPlayers[args.Who] = new PvPPlayer(args.Who);
        }
        private async void OnPlayerLogin(TShockAPI.Hooks.PlayerPostLoginEventArgs args)
        {
            try
            {
                await System.Threading.Tasks.Task.Delay(1500);

                if (args.Player != null && args.Player.Active)
                {
                    Handlers.PvPTerraInventory.CheckAndRecoverInventoryOnLogin(args.Player);
                }
            }
            catch (Exception ex)
            {
                TShock.Log.ConsoleError($"[PvPterraUtils] Error al recuperar inventario en el login: {ex.Message}");
            }
        }

        public static System.Collections.Generic.List<int> ParseTeamVacancies(string mode)
        {
            var vacancies = new System.Collections.Generic.List<int>();
            if (string.IsNullOrWhiteSpace(mode)) return vacancies;

            string m = mode.ToLower().Replace(" ", "");
            string separator = m.Contains("vs") ? "vs" : (m.Contains("v") ? "v" : "");

            if (string.IsNullOrEmpty(separator)) return vacancies;

            var parts = m.Split(new string[] { separator }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var part in parts)
            {
                if (int.TryParse(part, out int num)) vacancies.Add(num);
            }
            return vacancies;
        }

        public static bool AssignPlayerTeam(PvPPlayer pvpPlayer, string mode, string regionName)
        {
            var maxPlayersPerTeam = ParseTeamVacancies(mode);

            if (maxPlayersPerTeam == null || maxPlayersPerTeam.Count == 0)
            {
                pvpPlayer.Team = "None";
                return true;
            }

            string[] availableColors = { "Red", "Blue", "Green", "Yellow", "Pink" };

            var playersInRegion = PvPPlayers
                .Where(p => p != null && p.IsInPvP && p.CurrentRegion == regionName && !p.IsSpectator && p.TSPlayer.Index != pvpPlayer.TSPlayer.Index)
                .ToList();

            bool isGlobalPvP = string.IsNullOrEmpty(regionName) && Handlers.PvPTerraCommands.GlobalPvPActive;
            bool forceTeam = isGlobalPvP && Handlers.PvPTerraCommands.ForceTeamMode;
            bool ignoreTeam = isGlobalPvP && Handlers.PvPTerraCommands.IgnoreTeamMode;

            bool hasPreference = !string.IsNullOrEmpty(pvpPlayer.PreferredTeam) &&
                                 !pvpPlayer.PreferredTeam.Equals("none", StringComparison.OrdinalIgnoreCase) &&
                                 !pvpPlayer.PreferredTeam.Equals("spectator", StringComparison.OrdinalIgnoreCase);

            if (hasPreference && !ignoreTeam)
            {
                int teamIndex = Array.FindIndex(availableColors, c => c.Equals(pvpPlayer.PreferredTeam, StringComparison.OrdinalIgnoreCase));

                if (teamIndex != -1)
                {
                    if (forceTeam)
                    {
                        pvpPlayer.Team = availableColors[teamIndex];
                        return true;
                    }

                    int maxCapacity = maxPlayersPerTeam[0];

                    int currentCount = playersInRegion.Count(p => p.Team.Equals(availableColors[teamIndex], StringComparison.OrdinalIgnoreCase));

                    if (currentCount < maxCapacity)
                    {
                        pvpPlayer.Team = availableColors[teamIndex];
                        return true;
                    }
                }
            }

            int bestTeamIndex = -1;
            int minPlayers = int.MaxValue;

            for (int i = 0; i < maxPlayersPerTeam.Count && i < availableColors.Length; i++)
            {
                string colorToTry = availableColors[i];
                int currentCount = playersInRegion.Count(p => p.Team.Equals(colorToTry, StringComparison.OrdinalIgnoreCase));

                if ((currentCount < maxPlayersPerTeam[i] || forceTeam) && currentCount < minPlayers)
                {
                    minPlayers = currentCount;
                    bestTeamIndex = i;
                }
            }

            if (bestTeamIndex != -1)
            {
                pvpPlayer.Team = availableColors[bestTeamIndex];

                if (hasPreference && !ignoreTeam && !forceTeam)
                {
                    pvpPlayer.TSPlayer.SendWarningMessage(PvPTerrai18n.GetString("Core_TeamReassigned", pvpPlayer.PreferredTeam, pvpPlayer.Team));
                }

                return true;
            }

            return false;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                ServerApi.Hooks.ServerLeave.Deregister(this, OnPlayerLeave);
                ServerApi.Hooks.GameUpdate.Deregister(this, Handlers.PvPTerraRegion.OnGameUpdate);
                ServerApi.Hooks.ServerJoin.Deregister(this, OnServerJoin);
                ServerApi.Hooks.NetGetData.Deregister(this, Handlers.PvPTerraCombat.OnGetData);
                TShockAPI.Hooks.PlayerHooks.PlayerPostLogin -= OnPlayerLogin;
                Database.Shutdown();
            }
            base.Dispose(disposing);
        }

        private void InfoCommand(CommandArgs args) { Handlers.PvPTerraCommands.InfoCommand(args); }
        private void RankingCommand(CommandArgs args) { Handlers.PvPTerraCommands.RankingCommand(args); }
        private void RegionCommand(CommandArgs args) { Handlers.PvPTerraCommands.RegionCommand(args); }
        private void ActiveCommand(CommandArgs args) { Handlers.PvPTerraCommands.ActiveCommand(args); }
        private void TeamCommand(CommandArgs args) { Handlers.PvPTerraCommands.TeamCommand(args); }
    }
}