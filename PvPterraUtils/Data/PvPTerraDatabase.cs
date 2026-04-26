using Microsoft.Data.Sqlite;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using TShockAPI;
using TShockAPI.DB;

namespace PvPterraUtils.Data
{
    public struct KillEvent
    {
        public string KillerAccount;
        public string VictimAccount;
        public string Region;
    }

    public class PvPTerraDatabase
    {
        private IDbConnection _db;
        private ConcurrentQueue<KillEvent> _killQueue;
        private ConcurrentDictionary<string, DateTime> _recentKills;
        private CancellationTokenSource _cts;

        public PvPTerraDatabase()
        {
            string pluginFolder = Path.Combine(TShock.SavePath, "PvPterraConfig");

            if (!Directory.Exists(pluginFolder))
                Directory.CreateDirectory(pluginFolder);

            string dbPath = Path.Combine(pluginFolder, "pvpterra.sqldb");

            _db = new Microsoft.Data.Sqlite.SqliteConnection($"Data Source={dbPath}");

            _db.Open();

            InitializeDatabase();
            _killQueue = new ConcurrentQueue<KillEvent>();
            _recentKills = new ConcurrentDictionary<string, DateTime>();
            _cts = new CancellationTokenSource();

            Task.Run(() => ProcessQueueAsync(_cts.Token));
        }

        private void InitializeDatabase()
        {
            string createTableQuery = @"
                CREATE TABLE IF NOT EXISTS PvPRankingV2 (
                    AccountName VARCHAR(50),
                    Region VARCHAR(50),
                    Kills INTEGER DEFAULT 0,
                    Deaths INTEGER DEFAULT 0,
                    Score INTEGER DEFAULT 0,
                    PRIMARY KEY (AccountName, Region)
                );";
            _db.Query(createTableQuery);
        }

        public void EnqueueKill(string killer, string victim, string region)
        {
            _killQueue.Enqueue(new KillEvent { KillerAccount = killer, VictimAccount = victim, Region = region });
        }

        private async Task ProcessQueueAsync(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                if (_killQueue.TryDequeue(out var killEvent))
                {
                    try
                    {
                        string farmKey = $"{killEvent.KillerAccount}_{killEvent.VictimAccount}";
                        if (_recentKills.TryGetValue(farmKey, out DateTime lastKill))
                        {
                            if ((DateTime.Now - lastKill).TotalMinutes < 5) continue;
                        }
                        _recentKills[farmKey] = DateTime.Now;

                        _db.Query("INSERT INTO PvPRankingV2 (AccountName, Region, Kills, Score) VALUES (@0, @1, 1, 1) " +
                                  "ON CONFLICT(AccountName, Region) DO UPDATE SET Kills = Kills + 1, Score = Score + 1",
                                  killEvent.KillerAccount, killEvent.Region);

                        _db.Query("INSERT INTO PvPRankingV2 (AccountName, Region, Deaths, Score) VALUES (@0, @1, 1, -1) " +
                                  "ON CONFLICT(AccountName, Region) DO UPDATE SET Deaths = Deaths + 1, Score = Score - 1",
                                  killEvent.VictimAccount, killEvent.Region);
                    }
                    catch (Exception ex)
                    {
                        TShock.Log.ConsoleError($"[PvPterraUtils] Error SQLite: {ex.Message}");
                    }
                }
                else
                {
                    try
                    {
                        await Task.Delay(500, token);
                    }
                    catch (TaskCanceledException)
                    {
                        break;
                    }
                }
            }
        }

        public List<string> GetTopPlayers(int limit, string region = "")
        {
            var topPlayers = new List<string>();
            string query;

            if (string.IsNullOrEmpty(region))
            {
                query = $"SELECT AccountName, SUM(Score) as Score, SUM(Kills) as Kills, SUM(Deaths) as Deaths FROM PvPRankingV2 GROUP BY AccountName ORDER BY Score DESC LIMIT {limit}";
            }
            else
            {
                query = $"SELECT AccountName, Score, Kills, Deaths FROM PvPRankingV2 WHERE Region = @0 ORDER BY Score DESC LIMIT {limit}";
            }

            using (var reader = _db.QueryReader(query, region))
            {
                int rank = 1;
                while (reader.Read())
                {
                    string name = reader.Get<string>("AccountName");
                    int score = reader.Get<int>("Score");
                    int kills = reader.Get<int>("Kills");
                    int deaths = reader.Get<int>("Deaths");

                    string styledName = "";
                    string prefix = "";

                    switch (rank)
                    {
                        case 1:
                            styledName = GetRainbowText(name); 
                            prefix = "1. [i:4601]";            
                            break;
                        case 2:
                            styledName = $"[c/DDA0DD:{name}]"; 
                            prefix = "2. [i:4600]";            
                            break;
                        case 3:
                            styledName = $"[c/90EE90:{name}]"; 
                            prefix = "3. [i:4599]";            
                            break;
                        default:
                            styledName = $"[c/FFFFFF:{name}]"; 
                            prefix = $"{rank}. ";
                            break;
                    }

                    topPlayers.Add($"{prefix}{styledName} | {score}pts (K:{kills} D:{deaths})");
                    rank++;
                }
            }
            return topPlayers;
        }

        private string GetRainbowText(string text)
        {
            string[] hexColors = { "FF0000", "FF7F00", "FFFF00", "00FF00", "0000FF", "4B0082", "9400D3" };
            string result = "";
            for (int i = 0; i < text.Length; i++)
            {
                if (text[i] == ' ') { result += " "; continue; }
                result += $"[c/{hexColors[i % hexColors.Length]}:{text[i]}]";
            }
            return result;
        }

        public void Shutdown()
        {
            _cts.Cancel();
            _db.Dispose();
        }
    }
}