using PvPterraUtils.Data;
using PvPterraUtils.Models;
using PvPterraUtils.utils; 
using System;
using System.Collections.Generic;
using System.IO;
using Terraria;
using TShockAPI;
using TSUtils = TShockAPI.Utils;

namespace PvPterraUtils.Handlers
{
    public class PvPTerraInventory
    {
        public static bool TryTakeEscrow(TSPlayer player, string costString)
        {
            if (string.IsNullOrWhiteSpace(costString) || costString.ToLower() == "none")
                return true;

            int totalCopperCost = Utils.PvPTerraMisc.ParseCoinString(costString);
            if (totalCopperCost <= 0) return true;

            long playerTotalCopper = 0;
            for (int i = 50; i < 54; i++)
            {
                var item = player.TPlayer.inventory[i];
                if (item.type == 71) playerTotalCopper += item.stack;
                else if (item.type == 72) playerTotalCopper += item.stack * 100;  
                else if (item.type == 73) playerTotalCopper += item.stack * 10000;   
                else if (item.type == 74) playerTotalCopper += (long)item.stack * 1000000; 
            }

            if (playerTotalCopper < (long)totalCopperCost)
                return false;

            long remaining = playerTotalCopper - totalCopperCost;

            for (int i = 50; i < 54; i++)
            {
                player.TPlayer.inventory[i] = new Item();
                NetMessage.SendData((int)PacketTypes.PlayerSlot, -1, -1, null, player.Index, i);
            }

            int plat = (int)(remaining / 1000000);
            remaining %= 1000000;
            int gold = (int)(remaining / 10000);
            remaining %= 10000;
            int silver = (int)(remaining / 100);
            int copper = (int)(remaining % 100);

            if (copper > 0) { player.TPlayer.inventory[50].SetDefaults(71); player.TPlayer.inventory[50].stack = copper; }
            if (silver > 0) { player.TPlayer.inventory[51].SetDefaults(72); player.TPlayer.inventory[51].stack = silver; }
            if (gold > 0) { player.TPlayer.inventory[52].SetDefaults(73); player.TPlayer.inventory[52].stack = gold; }
            if (plat > 0) { player.TPlayer.inventory[53].SetDefaults(74); player.TPlayer.inventory[53].stack = plat; }

            for (int i = 50; i < 54; i++)
            {
                NetMessage.SendData((int)PacketTypes.PlayerSlot, -1, -1, null, player.Index, i);
            }

            return true;
        }
        public static bool HasEnoughEscrow(TSPlayer player, string costString)
        {
            if (string.IsNullOrWhiteSpace(costString) || costString.ToLower() == "none") return true;

            int totalCopperCost = Utils.PvPTerraMisc.ParseCoinString(costString);
            if (totalCopperCost <= 0) return true;

            long playerTotalCopper = 0;
            for (int i = 50; i < 54; i++) 
            {
                var item = player.TPlayer.inventory[i];
                if (item.type == 71) playerTotalCopper += item.stack;           
                else if (item.type == 72) playerTotalCopper += item.stack * 100;      
                else if (item.type == 73) playerTotalCopper += item.stack * 10000;   
                else if (item.type == 74) playerTotalCopper += (long)item.stack * 1000000; 
            }

            return playerTotalCopper >= totalCopperCost;
        }

        public static void ApplyPvPInventory(TSPlayer tsPlayer, RegionSettings regionConfig)
        {
            var pvpPlayer = PvPterraCore.PvPPlayers[tsPlayer.Index];

            pvpPlayer.OriginalStatLifeMax = tsPlayer.TPlayer.statLifeMax;
            pvpPlayer.OriginalStatManaMax = tsPlayer.TPlayer.statManaMax;

            tsPlayer.TPlayer.trashItem = new Item();
            NetMessage.SendData((int)PacketTypes.PlayerSlot, -1, -1, null, tsPlayer.Index, 499);

            if (regionConfig.PvPHealthActive)
            {
                int rawHealth = Math.Clamp(regionConfig.PvPHealth, 1, 500);
                int roundedHealth = ((rawHealth + 19) / 20) * 20;

                tsPlayer.TPlayer.statLifeMax = roundedHealth;
                tsPlayer.TPlayer.statLife = roundedHealth; 
            }

            if (regionConfig.PvPManaActive)
            {
                int rawMana = Math.Clamp(regionConfig.PvPMana, 1, 200);
                int roundedMana = ((rawMana + 19) / 20) * 20;

                tsPlayer.TPlayer.statManaMax = roundedMana;
                tsPlayer.TPlayer.statMana = roundedMana; 
            }

            NetMessage.SendData((int)PacketTypes.PlayerHp, -1, -1, null, tsPlayer.Index);
            NetMessage.SendData((int)PacketTypes.PlayerMana, -1, -1, null, tsPlayer.Index);

            for (int i = 0; i < 59; i++)
            {
                var item = tsPlayer.TPlayer.inventory[i];
                pvpPlayer.OriginalInventory[i] = (item != null && item.type != 0)
                    ? new NetItem(item.type, item.stack, item.prefix)
                    : new NetItem();

                tsPlayer.TPlayer.inventory[i] = new Item();
                NetMessage.SendData((int)PacketTypes.PlayerSlot, -1, -1, null, tsPlayer.Index, i);
            }

            tsPlayer.TPlayer.trashItem = new Item();

            if (PvPTerraJson.Config.CleanBuffsOnEnter)
            {
                for (int i = 0; i < Player.maxBuffs; i++)
                {
                    int bType = tsPlayer.TPlayer.buffType[i];
                    int bTime = tsPlayer.TPlayer.buffTime[i];

                    pvpPlayer.OriginalInventory[i + 200] = new NetItem(bType, bTime, 0);

                    tsPlayer.TPlayer.buffType[i] = 0;
                    tsPlayer.TPlayer.buffTime[i] = 0;
                }
                tsPlayer.SendData(PacketTypes.PlayerBuff, "", tsPlayer.Index);
            }

            for (int i = 0; i <= 9; i++)
            {
                var armorItem = tsPlayer.TPlayer.armor[i];
                pvpPlayer.OriginalInventory[i + 100] = (armorItem != null && armorItem.type != 0)
                    ? new NetItem(armorItem.type, armorItem.stack, armorItem.prefix)
                    : new NetItem();

                tsPlayer.TPlayer.armor[i] = new Item();
                NetMessage.SendData((int)PacketTypes.PlayerSlot, -1, -1, null, tsPlayer.Index, i + 59);
            }

            for (int i = 0; i <= 4; i++)
            {
                var miscItem = tsPlayer.TPlayer.miscEquips[i];
                pvpPlayer.OriginalInventory[i + 120] = (miscItem != null && miscItem.type != 0)
                    ? new NetItem(miscItem.type, miscItem.stack, miscItem.prefix)
                    : new NetItem();

                tsPlayer.TPlayer.miscEquips[i] = new Item();
                NetMessage.SendData((int)PacketTypes.PlayerSlot, -1, -1, null, tsPlayer.Index, i + 89);
            }

            Data.PvPTerraJson.SaveInventoryBackup(tsPlayer.Account.Name, pvpPlayer.OriginalInventory);

            var itemsList = (regionConfig.PvPinventoryItems?.Count > 0) ? regionConfig.PvPinventoryItems : Data.PvPTerraJson.Config.PvPinventoryItems;
            var potionsList = (regionConfig.PvPinventoryPotions?.Count > 0) ? regionConfig.PvPinventoryPotions : Data.PvPTerraJson.Config.PvPinventoryPotions;

            int currentSlot = 0;
            if (itemsList != null)
            {
                foreach (string itemData in itemsList)
                {
                    if (currentSlot > 49) break;

                    ParseItemData(itemData, out int itemId, out int stack);
                    if (itemId > 0)
                    {
                        tsPlayer.TPlayer.inventory[currentSlot].SetDefaults(itemId);
                        int maxStack = tsPlayer.TPlayer.inventory[currentSlot].maxStack;

                        tsPlayer.TPlayer.inventory[currentSlot].stack = (stack > 0 && stack <= maxStack) ? stack : maxStack;

                        NetMessage.SendData((int)PacketTypes.PlayerSlot, -1, -1, null, tsPlayer.Index, currentSlot);
                        currentSlot++;
                    }
                }
            }

            int potionSlot = 49;
            if (potionsList != null)
            {
                foreach (string potionData in potionsList)
                {
                    if (potionSlot < 0) break;

                    ParseItemData(potionData, out int potionId, out int stack);
                    if (potionId > 0 && tsPlayer.TPlayer.inventory[potionSlot].type == 0)
                    {
                        tsPlayer.TPlayer.inventory[potionSlot].SetDefaults(potionId);
                        int maxStack = tsPlayer.TPlayer.inventory[potionSlot].maxStack;

                        tsPlayer.TPlayer.inventory[potionSlot].stack = (stack > 0 && stack <= maxStack) ? stack : maxStack;

                        NetMessage.SendData((int)PacketTypes.PlayerSlot, -1, -1, null, tsPlayer.Index, potionSlot);
                    }
                    potionSlot--;
                }
            }
            if (!regionConfig.AllowPickUp)
            {

                tsPlayer.TPlayer.inventory[40].SetDefaults(4346);
                tsPlayer.TPlayer.inventory[40].stack = 1;

                NetMessage.SendData((int)PacketTypes.PlayerSlot, -1, -1, null, tsPlayer.Index, 40);

                tsPlayer.SendInfoMessage(PvPTerrai18n.GetString("Inv_StoneEquipped"));
            }
        }

        public static void ApplySpectatorInventory(PvPPlayer pvpPlayer)
        {
            var tsPlayer = pvpPlayer.TSPlayer;

            pvpPlayer.OriginalStatLifeMax = tsPlayer.TPlayer.statLifeMax;
            pvpPlayer.OriginalStatManaMax = tsPlayer.TPlayer.statManaMax;

            NetMessage.SendData((int)PacketTypes.PlayerHp, -1, -1, null, tsPlayer.Index);

            for (int i = 0; i < 59; i++)
            {
                var item = tsPlayer.TPlayer.inventory[i];
                pvpPlayer.OriginalInventory[i] = (item != null && item.type != 0)
                    ? new NetItem(item.type, item.stack, item.prefix) : new NetItem();
                tsPlayer.TPlayer.inventory[i] = new Item();
                NetMessage.SendData((int)PacketTypes.PlayerSlot, -1, -1, null, tsPlayer.Index, i);
            }
            for (int i = 0; i <= 9; i++)
            {
                var armorItem = tsPlayer.TPlayer.armor[i];
                pvpPlayer.OriginalInventory[i + 100] = (armorItem != null && armorItem.type != 0)
                    ? new NetItem(armorItem.type, armorItem.stack, armorItem.prefix) : new NetItem();
                tsPlayer.TPlayer.armor[i] = new Item();
                NetMessage.SendData((int)PacketTypes.PlayerSlot, -1, -1, null, tsPlayer.Index, i + 59);
            }
            for (int i = 0; i <= 4; i++)
            {
                var miscItem = tsPlayer.TPlayer.miscEquips[i];
                pvpPlayer.OriginalInventory[i + 120] = (miscItem != null && miscItem.type != 0)
                    ? new NetItem(miscItem.type, miscItem.stack, miscItem.prefix) : new NetItem();
                tsPlayer.TPlayer.miscEquips[i] = new Item();
                NetMessage.SendData((int)PacketTypes.PlayerSlot, -1, -1, null, tsPlayer.Index, i + 89);
            }

            Data.PvPTerraJson.SaveInventoryBackup(tsPlayer.Account.Name, pvpPlayer.OriginalInventory);

            int scryingOrbId = 5644;
            tsPlayer.TPlayer.inventory[0].SetDefaults(scryingOrbId);
            tsPlayer.TPlayer.inventory[0].stack = 1;
            NetMessage.SendData((int)PacketTypes.PlayerSlot, -1, -1, null, tsPlayer.Index, 0);
        }


        public static void CheckAndRecoverInventoryOnLogin(TSPlayer tsPlayer)
        {
            if (tsPlayer == null || !tsPlayer.IsLoggedIn) return;

            var backup = Data.PvPTerraJson.LoadInventoryBackup(tsPlayer.Account.Name);

            if (backup != null)
            {
                var pvpPlayer = PvPterraCore.PvPPlayers[tsPlayer.Index];

                if (pvpPlayer != null)
                {
                    RestoreOriginalInventory(pvpPlayer);

                    if (TShock.ServerSideCharacterConfig.Settings.Enabled)
                    {
                        TShock.CharacterDB.InsertPlayerData(tsPlayer);
                    }

                    tsPlayer.SendSuccessMessage(PvPTerrai18n.GetString("Inv_RecoverSuccess"));
                }
            }
        }

        public static void RestoreOriginalInventory(PvPPlayer pvpPlayer)
        {
            var tsPlayer = pvpPlayer.TSPlayer;

            tsPlayer.TPlayer.statLifeMax = pvpPlayer.OriginalStatLifeMax;
            tsPlayer.TPlayer.statManaMax = pvpPlayer.OriginalStatManaMax;

            tsPlayer.TPlayer.trashItem = new Item();
            NetMessage.SendData((int)PacketTypes.PlayerSlot, -1, -1, null, tsPlayer.Index, 499);

            NetMessage.SendData((int)PacketTypes.PlayerHp, -1, -1, null, tsPlayer.Index);
            NetMessage.SendData((int)PacketTypes.PlayerMana, -1, -1, null, tsPlayer.Index);

            var backup = pvpPlayer.OriginalInventory;



            if (backup == null || backup[0].NetId == 0)
                backup = PvPTerraJson.LoadInventoryBackup(tsPlayer.Account.Name);


            if (backup != null)
            {
                {
                    for (int i = 0; i < 59; i++)
                    {
                        SetAndSync(tsPlayer, i, backup[i], 0);
                    }

                    var savedTrash = backup[99];
                    if (savedTrash.NetId != 0)
                    {
                        tsPlayer.TPlayer.trashItem.netDefaults(savedTrash.NetId);
                        tsPlayer.TPlayer.trashItem.stack = savedTrash.Stack;
                        tsPlayer.TPlayer.trashItem.prefix = savedTrash.PrefixId;
                    }
                    else
                    {
                        tsPlayer.TPlayer.trashItem = new Item();
                    }

                    for (int i = 0; i < Player.maxBuffs; i++)
                    {
                        tsPlayer.TPlayer.buffType[i] = 0;
                        tsPlayer.TPlayer.buffTime[i] = 0;
                    }
                    NetMessage.SendData((int)PacketTypes.PlayerBuff, -1, -1, null, tsPlayer.Index);

                    for (int i = 0; i < Player.maxBuffs; i++)
                    {
                        var savedBuff = backup[i + 200];
                        if (savedBuff.NetId != 0)
                        {
                            tsPlayer.TPlayer.AddBuff(savedBuff.NetId, savedBuff.Stack, false);
                        }
                    }
                    NetMessage.SendData((int)PacketTypes.PlayerBuff, -1, -1, null, tsPlayer.Index);

                    for (int i = 0; i < 10; i++)
                    {
                        SetAndSyncArmor(tsPlayer, i, backup[i + 100]);
                    }

                    tsPlayer.SendData(PacketTypes.PlayerBuff, "", tsPlayer.Index);
                    for (int i = 0; i < 5; i++)
                    {
                        SetAndSyncMisc(tsPlayer, i, backup[i + 120]);
                    }

                    PvPTerraJson.DeleteInventoryBackup(tsPlayer.Account.Name);
                    tsPlayer.SendSuccessMessage(PvPTerrai18n.GetString("Inv_RestoreSuccess"));

                    if (pvpPlayer.PendingCoinReward > 0)
                    {
                        InjectCoins(tsPlayer, pvpPlayer.PendingCoinReward);

                        string iconEarnings = Utils.PvPTerraMisc.CopperToIconTag(pvpPlayer.PendingCoinReward);
                        tsPlayer.SendSuccessMessage(PvPTerrai18n.GetString("Inv_RewardPending", iconEarnings));

                        pvpPlayer.PendingCoinReward = 0;
                    }
                }
            }
        }
        public static void InjectCoins(TSPlayer player, int addedCopper)
        {
            if (addedCopper <= 0) return;

            long currentCopper = 0;
            for (int i = 50; i < 54; i++)
            {
                var item = player.TPlayer.inventory[i];
                if (item.type == 71) currentCopper += item.stack;
                else if (item.type == 72) currentCopper += item.stack * 100;
                else if (item.type == 73) currentCopper += item.stack * 10000;
                else if (item.type == 74) currentCopper += (long)item.stack * 1000000;

                player.TPlayer.inventory[i] = new Item(); 
            }

            long total = currentCopper + addedCopper;

            int plat = (int)(total / 1000000);
            total %= 1000000;
            int gold = (int)(total / 10000);
            total %= 10000;
            int silver = (int)(total / 100);
            int copper = (int)(total % 100);

            if (copper > 0) { player.TPlayer.inventory[50].SetDefaults(71); player.TPlayer.inventory[50].stack = copper; }
            if (silver > 0) { player.TPlayer.inventory[51].SetDefaults(72); player.TPlayer.inventory[51].stack = silver; }
            if (gold > 0) { player.TPlayer.inventory[52].SetDefaults(73); player.TPlayer.inventory[52].stack = gold; }
            if (plat > 0) { player.TPlayer.inventory[53].SetDefaults(74); player.TPlayer.inventory[53].stack = plat; }

            for (int i = 50; i < 54; i++)
                NetMessage.SendData((int)PacketTypes.PlayerSlot, -1, -1, null, player.Index, i);
        }

        private static void SetAndSync(TSPlayer plr, int slot, NetItem ni, int offset)
        {
            if (ni.NetId != 0)
            {
                plr.TPlayer.inventory[slot].netDefaults(ni.NetId);
                plr.TPlayer.inventory[slot].stack = ni.Stack;
                plr.TPlayer.inventory[slot].prefix = ni.PrefixId;
            }
            else
            {
                plr.TPlayer.inventory[slot] = new Item();
            }
            NetMessage.SendData((int)PacketTypes.PlayerSlot, -1, -1, null, plr.Index, slot + offset);
        }

        private static void SetAndSyncArmor(TSPlayer plr, int slot, NetItem ni)
        {
            if (ni.NetId != 0)
            {
                plr.TPlayer.armor[slot].netDefaults(ni.NetId);
                plr.TPlayer.armor[slot].stack = ni.Stack;
                plr.TPlayer.armor[slot].prefix = ni.PrefixId;
            }
            else
            {
                plr.TPlayer.armor[slot] = new Item();
            }
            NetMessage.SendData((int)PacketTypes.PlayerSlot, -1, -1, null, plr.Index, slot + 59);
        }

        private static void SetAndSyncMisc(TSPlayer plr, int slot, NetItem ni)
        {
            if (ni.NetId != 0)
            {
                plr.TPlayer.miscEquips[slot].netDefaults(ni.NetId);
                plr.TPlayer.miscEquips[slot].stack = ni.Stack;
                plr.TPlayer.miscEquips[slot].prefix = ni.PrefixId;
            }
            else
            {
                plr.TPlayer.miscEquips[slot] = new Item();
            }
            NetMessage.SendData((int)PacketTypes.PlayerSlot, -1, -1, null, plr.Index, slot + 89);
        }

        public static void CheckAndRestock(PvPPlayer pvpPlayer, RegionSettings regionConfig)
        {
            var tsPlayer = pvpPlayer.TSPlayer;
            bool didRestock = false;

            int itemSeconds = ParseSeconds(regionConfig.PvPItemsRestore);
            if (itemSeconds > 0 && (DateTime.Now - pvpPlayer.LastItemRestoreTime).TotalSeconds >= itemSeconds)
            {
                var itemsList = (regionConfig.PvPinventoryItems?.Count > 0) ? regionConfig.PvPinventoryItems : Data.PvPTerraJson.Config.PvPinventoryItems;
                if (itemsList != null)
                {
                    foreach (string itemData in itemsList) 
                    {
                        if (RestockSingleItem(tsPlayer, itemData)) didRestock = true;
                    }
                }
                pvpPlayer.LastItemRestoreTime = DateTime.Now; 
            }

            int potionSeconds = ParseSeconds(regionConfig.PvPotionsRestore);
            if (potionSeconds > 0 && (DateTime.Now - pvpPlayer.LastPotionRestoreTime).TotalSeconds >= potionSeconds)
            {
                var potionsList = (regionConfig.PvPinventoryPotions?.Count > 0) ? regionConfig.PvPinventoryPotions : Data.PvPTerraJson.Config.PvPinventoryPotions;
                if (potionsList != null)
                {
                    foreach (string potionData in potionsList) 
                    {
                        if (RestockSingleItem(tsPlayer, potionData)) didRestock = true;
                    }
                }
                pvpPlayer.LastPotionRestoreTime = DateTime.Now; 
            }

            if (didRestock)
            {
                tsPlayer.SendInfoMessage(PvPTerrai18n.GetString("Inv_RestockSuccess"));
            }
        }

        private static bool RestockSingleItem(TSPlayer tsPlayer, string itemData)
        {
            ParseItemData(itemData, out int itemId, out int targetStack);
            if (itemId <= 0) return false;

            Item tempItem = new Item();
            tempItem.SetDefaults(itemId);
            int max = (targetStack > 0 && targetStack <= tempItem.maxStack) ? targetStack : tempItem.maxStack;

            for (int i = 0; i <= 49; i++)
            {
                if (tsPlayer.TPlayer.inventory[i].type == itemId)
                {
                    if (tsPlayer.TPlayer.inventory[i].stack < max)
                    {
                        tsPlayer.TPlayer.inventory[i].stack = max;
                        NetMessage.SendData((int)PacketTypes.PlayerSlot, -1, -1, null, tsPlayer.Index, i);
                        return true;
                    }
                    return false;
                }
            }

            for (int i = 0; i <= 49; i++)
            {
                if (tsPlayer.TPlayer.inventory[i].type == 0) 
                {
                    tsPlayer.TPlayer.inventory[i].SetDefaults(itemId);
                    tsPlayer.TPlayer.inventory[i].stack = max;
                    NetMessage.SendData((int)PacketTypes.PlayerSlot, -1, -1, null, tsPlayer.Index, i);
                    return true;
                }
            }
            return false;
        }

        private static int ParseSeconds(string timeStr)
        {
            if (string.IsNullOrWhiteSpace(timeStr) || timeStr.ToLower() == "none") return 0;
            string lower = timeStr.Trim().ToLower();
            if (lower.EndsWith("s")) return int.TryParse(lower.TrimEnd('s'), out int s) ? s : 0;
            if (lower.EndsWith("m")) return int.TryParse(lower.TrimEnd('m'), out int m) ? m * 60 : 0;
            return int.TryParse(lower, out int res) ? res : 0;
        }

        public static void ParseItemData(string data, out int itemId, out int stack)
        {
            itemId = 0; stack = 0;
            if (string.IsNullOrWhiteSpace(data)) return;

            var parts = data.Split(':');
            if (int.TryParse(parts[0], out int parsedId)) itemId = parsedId;
            if (parts.Length > 1 && int.TryParse(parts[1], out int parsedStack)) stack = parsedStack;
        }
    }
}