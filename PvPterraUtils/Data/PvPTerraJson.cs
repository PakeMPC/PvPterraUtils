using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using TShockAPI;
using Terraria;
using PvPterraUtils.utils;

namespace PvPterraUtils.Data
{
    public class PvPConfig
    {
        public string PvPdefaultLang = "en";
        public string DefaultReward = "none";
        public bool AllowBackOnDeath = false;
        [JsonProperty(ObjectCreationHandling = ObjectCreationHandling.Replace)]
        public List<int> DefaultPvPBuffs = new List<int> { 11, 12 };
        public bool CleanBuffsOnEnter = true;
        public int CombatTagDurationSeconds = 15;
        public bool PvPHealthActive = false;
        public int PvPHealth = 400; 
        public bool PvPManaActive = false;
        public int PvPMana = 200; 


        public bool PvPinventoryActive = false;
        public List<string> PvPinventoryItems = new List<string>();
        public string PvPItemsRestore = "none";
        public List<string> PvPinventoryPotions = new List<string>();
        public string PvPotionsRestore = "none";

        public bool AllowPickUp = true;
        public bool AllowDrop = true;

        public Dictionary<string, RegionSettings> RegionConfigs = new Dictionary<string, RegionSettings>();
    }

    public class RegionSettings
    {
        public string Mode = "FFA";
        public string PvPreward = "none";
        public bool AllowBackOnDeath = false;
        public List<int> PvPbuffs = new List<int>();
        public bool PvPHealthActive = false;
        public int PvPHealth = 400;
        public bool PvPManaActive = false;
        public int PvPMana = 200;

        public bool PvPinventoryActive = false;
        public List<string> PvPinventoryItems = new List<string>();
        public string PvPItemsRestore = "none";
        public List<string> PvPinventoryPotions = new List<string>();
        public string PvPotionsRestore = "none";

        public bool AllowPickUp = false;
        public bool AllowDrop = false;
    }

    public class InventoryBackup
    {
        public string AccountName { get; set; }
        public int OriginalLife { get; set; }  
        public int OriginalMana { get; set; }  
        public NetItem[] Items { get; set; }
    }

    public static class PvPTerraJson
    {
        public static string ConfigDirectory = Path.Combine(TShock.SavePath, "PvPterraConfig");
        public static string ConfigPath = Path.Combine(ConfigDirectory, "PvPterraConfig.json");
        public static string BackupsDirectory = Path.Combine(ConfigDirectory, "Backups");

        public static PvPConfig Config;

        public static void LoadConfig()
        {
            try
            {
                if (!Directory.Exists(ConfigDirectory))
                    Directory.CreateDirectory(ConfigDirectory);

                if (!Directory.Exists(BackupsDirectory))
                    Directory.CreateDirectory(BackupsDirectory);

                if (!File.Exists(ConfigPath))
                {
                    TShock.Log.ConsoleInfo(PvPTerrai18n.GetString("Log_ConfigNew"));
                    Config = new PvPConfig();
                    SaveConfig();
                    return;
                }

                string json = File.ReadAllText(ConfigPath);
                Config = JsonConvert.DeserializeObject<PvPConfig>(json);

                if (Config == null) Config = new PvPConfig();
                if (Config.RegionConfigs == null) Config.RegionConfigs = new Dictionary<string, RegionSettings>();

                PvPTerrai18n.CurrentLang = Config.PvPdefaultLang.ToLower();
                SaveConfig();
            }
            catch (Exception ex)
            {
                TShock.Log.ConsoleError(PvPTerrai18n.GetString("Log_ConfigLoadError", ex.Message));
                Config = new PvPConfig();
            }
        }

        public static void SaveConfig()
        {
            try
            {
                if (!Directory.Exists(ConfigDirectory))
                    Directory.CreateDirectory(ConfigDirectory);

                string json = JsonConvert.SerializeObject(Config, Formatting.Indented);
                File.WriteAllText(ConfigPath, json);
            }
            catch (Exception ex)
            {
                TShock.Log.ConsoleError(PvPTerrai18n.GetString("Log_ConfigSaveError", ex.Message));
            }
        }

        public static void SaveInventoryBackup(string accountName, NetItem[] inventory, int life, int mana)
        {
            if (string.IsNullOrWhiteSpace(accountName)) return;

            if (!Directory.Exists(BackupsDirectory))
                Directory.CreateDirectory(BackupsDirectory);

            string backupPath = Path.Combine(BackupsDirectory, $"{accountName}.json");
            var backup = new InventoryBackup { AccountName = accountName, Items = inventory, OriginalLife = life, OriginalMana = mana };

            string json = JsonConvert.SerializeObject(backup, Formatting.Indented);
            File.WriteAllText(backupPath, json);
        }

        public static InventoryBackup LoadInventoryBackup(string accountName)
        {
            string backupPath = Path.Combine(BackupsDirectory, $"{accountName}.json");
            if (File.Exists(backupPath))
            {
                string json = File.ReadAllText(backupPath);
                return JsonConvert.DeserializeObject<InventoryBackup>(json);
            }
            return null;
        }

        public static void DeleteInventoryBackup(string accountName)
        {
            string backupPath = Path.Combine(BackupsDirectory, $"{accountName}.json");
            if (File.Exists(backupPath))
            {
                File.Delete(backupPath);
            }
        }
    }
}