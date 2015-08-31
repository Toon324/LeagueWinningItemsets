using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Web;
using LeagueStatisticallyBestItemset.Models;
using Microsoft.WindowsAzure.Storage.Blob;
using Newtonsoft.Json;

namespace LeagueStatisticallyBestItemset.Services
{
    public class ItemSetService
    {
        private Dictionary<string, ItemStats> _itemStats = new Dictionary<string, ItemStats>();

        /// <summary>
        /// Returns a List of ints that represent Match Id's from a file in the matches Blob storage container
        /// </summary>
        /// <param name="filename">Name of file that contains a JSONified list of Ints</param>
        /// <returns>List of ints</returns>
        public List<int> LoadMatchsetFromFile(string filename)
        {
            try
            {
                var container = ApiTools.GetBlobContainer("matches");

                var blob = container.GetBlockBlobReference(filename);

                if (!blob.Exists())
                    return new List<int>();

                var json = blob.DownloadText();

                var toReturn = JsonConvert.DeserializeObject<int[]>(json);
                Logger.LogMessageToFile(GetType().Name, "Matchset loaded from file");

                return toReturn.ToList();

            }
            catch (Exception e)
            {
                Logger.LogMessageToFile(GetType().Name, e.Message);
                return new List<int>();
            }
        }

        /// <summary>
        /// Iterates through a list of Matches to generate Item Stats.
        /// Each Match's timeline of purchases is loaded and built into a list of ItemStats.
        /// These stats are tracked per Item, per Champion, per Lane.
        /// </summary>
        /// <param name="region">Region of data</param>
        /// <param name="matches">List of Matches</param>
        public void GenerateStatsFromMatchset(string region, List<MatchDetail> matches)
        {
            var processed = 0;
            var total = matches.Count;
            foreach (var match in matches)
            {
                processed++;
                Logger.LogMessageToFile(GetType().Name, "Loading stats from Match " + match.MatchId + "   Progress: " + processed.PercentOf(total) + "%");

                if (match.Participants == null || !match.Participants.Any())
                {
                    Logger.LogMessageToFile(GetType().Name, "WARNING! No participants found in Match " + match.MatchId);
                    continue;
                }

                var allPurchases = match.GetAllItemPurchases();

                foreach (var player in match.Participants)
                {
                    var purchases = allPurchases.Where(x => x.ParticipantId == player.ParticipantId).ToList();


                    foreach (var purchase in purchases)
                    {
                        UpdateItemStats(purchase.ItemId,
                            purchase.Timestamp,
                            player.ChampionId,
                            player.Timeline.Lane,
                            match.ParticipantWon(player),
                            0);
                    }
                }
            }
        }
        
        /// <summary>
        /// Writes all Stats to Cache Files.
        /// Will not write data if Stats have not been generated.
        /// </summary>
        /// <param name="region">Region of Data</param>
        /// <param name="uniqueName">Optional unique identifier for this data. Useful for generating custom stats.</param>
        public void WriteStatsToFile(string region, string uniqueName = "")
        {
            var stats = GetAllItemStats();
            if (stats == null || !stats.Any())
                return;

            foreach (var itemStat in stats.OrderBy(x => x.ChampionId).ThenBy(x => x.WinPercent))
            {
                CacheService<ItemStats>.WriteToCache("StatsCache", region + uniqueName, itemStat.ChampionId + "_" + itemStat.Lane + "_" + itemStat.ItemId,
                    itemStat);
            }
        }

        /// <summary>
        /// Updates the ItemStat with either a win or a loss.
        /// </summary>
        private void UpdateItemStats(int itemId, long timestamp, int championId, string lane, bool didWin, int itemCount)
        {
            var key = championId + "_" + lane + "_" + itemId;

            if (!_itemStats.ContainsKey(key))
                _itemStats.Add(key, new ItemStats()
                {
                    ItemId = itemId,
                    ChampionId = championId,
                    Lane = lane
                });

            var stats = _itemStats[key];

            stats.Uses++;

            // Keep a running average of when the item was bought
            stats.AverageTimeBought += (timestamp - stats.AverageTimeBought) / stats.Uses;

            if (!didWin) return;

            stats.Wins++;
        }

        /// <summary>
        /// Returns a list of all item stats.
        /// Will return an empty list if stats have not been generated.
        /// </summary>
        /// <returns>List of ItemStats</returns>
        public List<ItemStats> GetAllItemStats()
        {
            return _itemStats.Values.ToList();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="region"></param>
        /// <param name="champId"></param>
        /// <param name="role"></param>
        /// <returns></returns>
        public ItemSet GetItemset(string region, int champId, string role)
        {
            var lane = role.ToUpper();

            var cachedSet = CacheService<ItemSet>.GetFromCache("itemsetcache", region + "_" + champId + "_" + lane + ".json");

            if (cachedSet != null)
                return cachedSet;

            var itemStats = CacheService<ItemStats>.GetListFromCache("statscache", region, champId, lane).ToList();

            var itemset = new ItemSet(region, itemStats);

            CacheService<ItemSet>.WriteToCache("itemsetcache", region, champId + "_" + lane, itemset);

            return itemset;
        }

        /// <summary>
        /// Used to upload the Template file for ItemSets
        /// </summary>
        public static void UploadItemSetTemplate()
        {
            var container = ApiTools.GetBlobContainer("itemsets");

            var blob = container.GetBlockBlobReference("ItemSetTemplate.txt");

            using (var r = File.OpenRead(ConfigurationManager.AppSettings["ItemSetTemplatePath"]))
            {
                blob.UploadFromStream(r);
            }
        }

        /// <summary>
        /// Gets an Itemset from the Azure Storage.
        /// Does not use CacheService due to custom naming schemes and performance optimizations.
        /// </summary>
        /// <param name="region">Region of data</param>
        /// <param name="champId">Id of champion</param>
        /// <param name="role">Role of champion</param>
        /// <returns></returns>
        public CloudBlockBlob GetItemsetBlob(string region, int champId, string role)
        {
            var container = ApiTools.GetBlobContainer("itemsets");

            var filename = "LWI_" + StaticDataService.GetChampion(region, champId).Name + "_" + role + ".json";

            var blob = container.GetBlockBlobReference(filename);

            if (!blob.Exists())
            {
                var itemSet = GetItemset(region, champId, role);
                var json = itemSet.GenerateJson();

                blob.UploadText(json);
            }

            return blob;
        }
    }
}