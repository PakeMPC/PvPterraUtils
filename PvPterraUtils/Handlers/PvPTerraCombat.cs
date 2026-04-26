using PvPterraUtils.Data;
using PvPterraUtils.Models;
using PvPterraUtils.utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Terraria;
using TerrariaApi.Server;
using TShockAPI;

namespace PvPterraUtils.Handlers
{
    public class PvPTerraCombat
    {

        public static void OnGetData(GetDataEventArgs args)
        {

            switch (args.MsgID)
            {
                case PacketTypes.Tile:
                    HandleTileEdit(args);
                    break;

                case PacketTypes.PlayerHurtV2:
                    if (args.Handled) return;
                    HandlePlayerDamage(args);
                    break;

                case PacketTypes.PlayerDeathV2:
                    if (args.Handled) return;
                    HandlePlayerDeath(args);
                    break;

                case PacketTypes.PlayerSlot:
                    HandleInventorySlot(args);
                    break;

                case (PacketTypes)147: 
                    HandleLoadoutChange(args);
                    break;

                case PacketTypes.PlayerTeam: 
                    HandleTeamChange(args);
                    break;

                case PacketTypes.TogglePvp: 
                    HandlePvPToggle(args);
                    break;

                case PacketTypes.ItemDrop: 
                    HandleItemDrop(args);
                    break;

                case PacketTypes.ItemOwner: 
                    HandleItemPickup(args);
                    break;
            }
        }

        private static void HandleTileEdit(GetDataEventArgs args)
        {
            var pvpPlayer = PvPterraCore.PvPPlayers[args.Msg.whoAmI];
            if (pvpPlayer == null || !pvpPlayer.IsInPvP) return;

            try
            {
                using (var reader = new System.IO.BinaryReader(new System.IO.MemoryStream(args.Msg.readBuffer, args.Index, args.Length)))
                {
                    byte action = reader.ReadByte();
                    short x = reader.ReadInt16();
                    short y = reader.ReadInt16();

                    if (!pvpPlayer.TSPlayer.HasBuildPermission(x, y))
                    {
                        args.Handled = true;

                        pvpPlayer.TSPlayer.SendTileSquareCentered(x, y, 3);
                    }
                }
            }
            catch (Exception)
            {
                args.Handled = true;
            }
        }

        private static void HandleTeamChange(GetDataEventArgs args)
        {
            var pvpPlayer = PvPterraCore.PvPPlayers[args.Msg.whoAmI];
            if (pvpPlayer == null || !pvpPlayer.IsInPvP) return;

            using (var reader = new BinaryReader(new MemoryStream(args.Msg.readBuffer, args.Index, args.Length)))
            {
                byte playerId = reader.ReadByte();
                byte newTeamId = reader.ReadByte(); 

                int assignedTeamId = Utils.PvPTerraMisc.GetTeamIdFromName(pvpPlayer.Team);

                if (newTeamId != assignedTeamId)
                {
                    args.Handled = true; 
                    pvpPlayer.TSPlayer.SendErrorMessage(PvPTerrai18n.GetString("Combat_TeamLocked", pvpPlayer.Team));

                    pvpPlayer.TSPlayer.TPlayer.team = assignedTeamId;

                    NetMessage.SendData((int)PacketTypes.PlayerTeam, -1, -1, null, pvpPlayer.TSPlayer.Index);
                }
            }
        }

        private static void HandleLoadoutChange(GetDataEventArgs args)
        {
            var pvpPlayer = PvPterraCore.PvPPlayers[args.Msg.whoAmI];
            if (pvpPlayer == null || !pvpPlayer.IsInPvP) return;

            args.Handled = true;

            pvpPlayer.TSPlayer.SendErrorMessage(PvPTerrai18n.GetString("Combat_LoadoutLocked"));

            pvpPlayer.TSPlayer.TPlayer.CurrentLoadoutIndex = 0;

            NetMessage.SendData(147, -1, -1, null, pvpPlayer.TSPlayer.Index);
        }

        private static void HandleInventorySlot(GetDataEventArgs args)
        {
            var pvpPlayer = PvPterraCore.PvPPlayers[args.Msg.whoAmI];
            if (pvpPlayer == null || !pvpPlayer.IsInPvP) return;

            using (var reader = new System.IO.BinaryReader(new System.IO.MemoryStream(args.Msg.readBuffer, args.Index, args.Length)))
            {
                byte playerId = reader.ReadByte();
                short slotId = reader.ReadInt16();
                short stack = reader.ReadInt16();
                byte prefix = reader.ReadByte();
                short netId = reader.ReadInt16();

                if (pvpPlayer.IsSpectator)
                {
                    args.Handled = true;
                    SyncFullInventory(pvpPlayer.TSPlayer);
                    return;
                }

                bool isStoneSlot = false;
                if (Data.PvPTerraJson.Config.RegionConfigs.TryGetValue(pvpPlayer.CurrentRegion, out var regionConfig))
                {
                    if (!regionConfig.AllowPickUp && slotId == 40) isStoneSlot = true;
                }
                bool isVanityOrDye = (slotId >= 69 && slotId <= 88) || (slotId >= 94 && slotId <= 98);

                if (isVanityOrDye || slotId == 499 || isStoneSlot)
                {
                    args.Handled = true;
                    SyncFullInventory(pvpPlayer.TSPlayer);
                    pvpPlayer.TSPlayer.SendErrorMessage(PvPTerrai18n.GetString("Combat_SlotLocked"));
                    return;
                }

                if (IsItemInInventorySlots(pvpPlayer.TSPlayer.TPlayer, netId, prefix))
                {
                    args.Handled = true;
                    SyncFullInventory(pvpPlayer.TSPlayer);
                    pvpPlayer.TSPlayer.SendErrorMessage(PvPTerrai18n.GetString("Combat_InventoryLocked"));
                    return;
                }
            }
        }

        private static bool IsItemInInventorySlots(Terraria.Player player, int netId, byte prefix)
        {
            if (netId == 0) return false;

            for (int i = 10; i < 20; i++)
            {
                var item = player.armor[i];
                if (item != null && item.type == netId && item.prefix == prefix) return true;
            }

            for (int i = 0; i < player.dye.Length; i++)
            {
                var item = player.dye[i];
                if (item != null && item.type == netId && item.prefix == prefix) return true;
            }

            for (int i = 0; i < player.miscDyes.Length; i++)
            {
                var item = player.miscDyes[i];
                if (item != null && item.type == netId && item.prefix == prefix) return true;
            }

            return false;
        }

        private static void SyncFullInventory(TSPlayer player)
        {
            for (int i = 0; i <= 98; i++)
                NetMessage.SendData((int)PacketTypes.PlayerSlot, -1, -1, null, player.Index, i);
        }

        private static void HandlePvPToggle(GetDataEventArgs args)
        {
            var pvpPlayer = PvPterraCore.PvPPlayers[args.Msg.whoAmI];
            if (pvpPlayer == null || !pvpPlayer.IsInPvP) return;

            args.Handled = true;
            pvpPlayer.TSPlayer.SendErrorMessage(PvPTerrai18n.GetString("Combat_PvPLocked"));

            Utils.PvPTerraMisc.SyncPvPState(pvpPlayer.TSPlayer, true, pvpPlayer.Team);
        }

        private static void HandlePlayerDamage(GetDataEventArgs args)
        {
            using (var reader = new BinaryReader(new MemoryStream(args.Msg.readBuffer, args.Index, args.Length)))
            {
                byte victimId = reader.ReadByte();
                var deathReason = Terraria.DataStructures.PlayerDeathReason.FromReader(reader);

                if (deathReason._sourcePlayerIndex >= 0 && deathReason._sourcePlayerIndex < Main.maxPlayers)
                {
                    int attackerId = deathReason._sourcePlayerIndex;

                    var victim = PvPterraCore.PvPPlayers[victimId];
                    var attacker = PvPterraCore.PvPPlayers[attackerId];

                    if (victim != null && victim.IsInPvP && attacker != null)
                    {
                        if (victim.IsSpectator)
                        {
                            args.Handled = true;
                            return;
                        }
                        victim.LastCombatTime = DateTime.Now;
                        victim.LastAttackerName = attacker.TSPlayer.Name;

                        attacker.LastCombatTime = DateTime.Now;
                    }
                }
            }
        }

        private static void HandlePlayerDeath(GetDataEventArgs args)
        {
            using (var reader = new BinaryReader(new MemoryStream(args.Msg.readBuffer, args.Index, args.Length)))
            {
                byte victimId = reader.ReadByte();
                var victim = PvPterraCore.PvPPlayers[victimId];
                if (victim == null || !victim.IsInPvP) return;

                var deathReason = Terraria.DataStructures.PlayerDeathReason.FromReader(reader);
                int killerId = deathReason._sourcePlayerIndex;

                if (killerId >= 0 && killerId < 255)
                {
                    var killerTS = TShock.Players[killerId];
                    var killerPvP = PvPterraCore.PvPPlayers[killerId];

                    if (killerTS != null && killerPvP != null)
                    {
                        killerPvP.KillStreak++;

                        CheckAndBroadcastStreak(killerPvP);

                        if (victim.KillStreak >= 3)
                        {
                            Utils.PvPTerraMisc.BroadcastToRegion(victim.CurrentRegion,
                                PvPTerrai18n.GetString("Streak_End", killerTS.Name, victim.TSPlayer.Name), 255, 100, 100);
                        }

                        string killerName = killerTS.Account?.Name ?? killerTS.Name;
                        string victimName = victim.TSPlayer.Account?.Name ?? victim.TSPlayer.Name;
                        PvPterraCore.Database.EnqueueKill(killerName, victimName, victim.CurrentRegion);
                        GiveKillReward(killerTS, victim);
                    }
                }

                Data.PvPTerraJson.Config.RegionConfigs.TryGetValue(victim.CurrentRegion, out var regionConfig);

                bool isFFA = regionConfig != null && regionConfig.Mode.Equals("FFA", StringComparison.OrdinalIgnoreCase);
                bool allowBack = (regionConfig != null && regionConfig.AllowBackOnDeath) || Data.PvPTerraJson.Config.AllowBackOnDeath;

                victim.KillStreak = 0;

                if (isFFA)
                {
                    if (!allowBack)
                    {
                        victim.NeedsInventoryRestore = true;
                        victim.IsInPvP = false;
                        victim.IsSpectator = false;
                        Utils.PvPTerraMisc.SyncPvPState(victim.TSPlayer, false, "None");
                    }
                    else
                    {
                        victim.PendingRespawnTeleport = true; 
                    }
                }
                else
                {
                    if (!allowBack) victim.IsSpectator = true;
                    else victim.PendingRespawnTeleport = true; 
                }

                string deathRegion = victim.CurrentRegion;
                System.Threading.Tasks.Task.Run(async () =>
                {
                    await System.Threading.Tasks.Task.Delay(150);
                    PvPTerraRegion.CheckMatchWinner(deathRegion);
                });
            }
        }

        private static void CheckAndBroadcastStreak(PvPPlayer killer)
        {
            string message = "";
            switch (killer.KillStreak)
            {
                case 3: message = PvPTerrai18n.GetString("Streak_3", killer.TSPlayer.Name); break;
                case 5: message = PvPTerrai18n.GetString("Streak_5", killer.TSPlayer.Name); break;
                case 10: message = PvPTerrai18n.GetString("Streak_10", killer.TSPlayer.Name); break;
                case 15: message = PvPTerrai18n.GetString("Streak_15", killer.TSPlayer.Name); break;
                case 20: message = PvPTerrai18n.GetString("Streak_20", killer.TSPlayer.Name); break;
            }

            if (!string.IsNullOrEmpty(message))
            {
                Utils.PvPTerraMisc.BroadcastToRegion(killer.CurrentRegion, message, 255, 215, 0);
            }
        }

        public static void GiveKillReward(TSPlayer killer, PvPPlayer victim)
        {
            if (!Data.PvPTerraJson.Config.RegionConfigs.TryGetValue(victim.CurrentRegion, out var regionConfig))
                return;

            string rewardString = regionConfig.PvPreward;

            if (string.IsNullOrWhiteSpace(rewardString) || rewardString.ToLower() == "none")
                return;

            int totalCopper = Utils.PvPTerraMisc.ParseCoinString(rewardString);
            if (totalCopper <= 0) return;

            var killerPvP = PvPterraCore.PvPPlayers[killer.Index];
            bool usedPvPInv = false;

            if (Data.PvPTerraJson.Config.RegionConfigs.TryGetValue(victim.CurrentRegion, out var config2))
                usedPvPInv = config2.PvPinventoryActive || Data.PvPTerraJson.Config.PvPinventoryActive;

            if (usedPvPInv && killerPvP != null)
            {
                killerPvP.PendingCoinReward += totalCopper;
            }
            else 
            {
                PvPTerraInventory.InjectCoins(killer, totalCopper);
            }

            string iconReward = Utils.PvPTerraMisc.CopperToIconTag(totalCopper);
            killer.SendSuccessMessage(PvPTerrai18n.GetString("Combat_KillReward", iconReward, victim.TSPlayer.Name));

            var greenColor = new Microsoft.Xna.Framework.Color(50, 255, 50);
            var goldColor = new Microsoft.Xna.Framework.Color(255, 215, 0);

            NetMessage.SendData((int)PacketTypes.CreateCombatTextExtended, -1, -1,
                Terraria.Localization.NetworkText.FromLiteral("+1 pts"),
                (int)greenColor.PackedValue, 
                killer.TPlayer.position.X,
                killer.TPlayer.position.Y - 32f);

            NetMessage.SendData((int)PacketTypes.CreateCombatTextExtended, -1, -1,
                Terraria.Localization.NetworkText.FromLiteral($"+{rewardString}"),
                (int)goldColor.PackedValue,
                killer.TPlayer.position.X,
                killer.TPlayer.position.Y);
        }

        private static void LogDeath(string killerName, string victimName, string region)
        {
            string logPath = Path.Combine(TShock.SavePath, "PvPterraConfig", "pvpterra.log");
            string logDirectory = Path.GetDirectoryName(logPath);

            if (!Directory.Exists(logDirectory))
                Directory.CreateDirectory(logDirectory);

            string logEntry = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {killerName} mató a {victimName} en la región {region}.";

            File.AppendAllText(logPath, logEntry + Environment.NewLine);


        }

        private static void HandleItemDrop(GetDataEventArgs args)
        {
            var pvpPlayer = PvPterraCore.PvPPlayers[args.Msg.whoAmI];
            if (pvpPlayer == null || !pvpPlayer.IsInPvP) return;

            if (!PvPTerraJson.Config.RegionConfigs.TryGetValue(pvpPlayer.CurrentRegion, out var regionConfig))
                return;

            if (regionConfig.AllowDrop) return;

            using (var reader = new BinaryReader(new MemoryStream(args.Msg.readBuffer, args.Index, args.Length)))
            {
                short itemId = reader.ReadInt16();

                if (itemId == 400)
                {
                    args.Handled = true;
                    pvpPlayer.TSPlayer.SendErrorMessage(PvPTerrai18n.GetString("Combat_DropLocked"));

                    for (int i = 0; i < 59; i++)
                        NetMessage.SendData((int)PacketTypes.PlayerSlot, -1, -1, null, pvpPlayer.TSPlayer.Index, i);

                    NetMessage.SendData((int)PacketTypes.PlayerSlot, -1, -1, null, pvpPlayer.TSPlayer.Index, 58);
                }
            }
        }

        private static void HandleItemPickup(GetDataEventArgs args)
        {
            var pvpPlayer = PvPterraCore.PvPPlayers[args.Msg.whoAmI];
            if (pvpPlayer == null || !pvpPlayer.IsInPvP) return;

            if (!Data.PvPTerraJson.Config.RegionConfigs.TryGetValue(pvpPlayer.CurrentRegion, out var regionConfig))
                return;

            if (regionConfig.AllowPickUp) return;

            using (var reader = new BinaryReader(new MemoryStream(args.Msg.readBuffer, args.Index, args.Length)))
            {
                short itemId = reader.ReadInt16();
                byte owner = reader.ReadByte();

                if (owner == pvpPlayer.TSPlayer.Index)
                {
                    if (itemId >= 0 && itemId < Main.maxItems)
                    {
                        var worldItem = Main.item[itemId];

                        if (worldItem != null && worldItem.active)
                        {
                            int type = worldItem.type;
                            if (type == 58 || type == 73 || type == 3453 || type == 3454 || type == 3455)
                            {
                                return;
                            }

                            args.Handled = true;
                            pvpPlayer.TSPlayer.SendErrorMessage(PvPTerrai18n.GetString("Combat_PickupLocked"));

                            for (int i = 0; i < 59; i++)
                            {
                                NetMessage.SendData((int)PacketTypes.PlayerSlot, -1, -1, null, pvpPlayer.TSPlayer.Index, i);
                            }

                            NetMessage.SendData((int)PacketTypes.ItemDrop, -1, -1, null, itemId);
                            NetMessage.SendData((int)PacketTypes.ItemOwner, -1, -1, null, itemId, 255);
                        }
                    }
                }
            }
        }

    }

}