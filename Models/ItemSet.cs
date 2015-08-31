using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.UI.WebControls;
using LeagueStatisticallyBestItemset.Services;

namespace LeagueStatisticallyBestItemset.Models
{
    public class ItemSet
    {
        public ItemSet(string region, IEnumerable<ItemStats> itemStats)
        {
            EarlyItems = new List<ItemStats>();
            MidgameItems = new List<ItemStats>();
            LategameItems = new List<ItemStats>();

            foreach (var stat in itemStats
                .GroupBy(x => x.ItemId)
                .Select(group => group.First())
                .Where(x => x.Wins > Convert.ToInt32(ConfigurationManager.AppSettings["ItemMinimumWinsRequired"]))
                .OrderByDescending(x => x.WinPercent))
            {
                if (Champion == null)
                    Champion = StaticDataService.GetChampion(region, stat.ChampionId);

                if (Lane == null)
                    Lane = stat.Lane;

                switch (stat.GetTimeSection())
                {
                    case ItemStats.Section.Early:
                        EarlyItems.Add(stat);
                        break;
                    case ItemStats.Section.Mid:
                        MidgameItems.Add(stat);
                        break;
                    default:
                        LategameItems.Add(stat);
                        break;
                }
            }

            // Limit number of items in each section

            var numberOfSectionItems = Convert.ToInt32(ConfigurationManager.AppSettings["ItemsPerSection"]);

            EarlyItems = EarlyItems.Take(numberOfSectionItems).ToList();
            MidgameItems = MidgameItems.Where(x => StaticDataService.GetItem(region, x.ItemId).IsFinalItem)
                .Take(numberOfSectionItems).ToList();
            LategameItems = LategameItems.Where(x => StaticDataService.GetItem(region, x.ItemId).IsFinalItem)
                .Take(numberOfSectionItems).ToList();
        }

        public List<ItemStats> EarlyItems { get; private set; }
        public List<ItemStats> MidgameItems { get; private set; }
        public List<ItemStats> LategameItems { get; private set; }

        public Champion Champion { get; private set; }

        public string Lane { get; private set; }

        public override string ToString()
        {
            return String.Format("ItemSet [ EarlyGame: {0}, MidGame: {1}, LateGame: {2} ]", EarlyItems.Count, MidgameItems.Count, LategameItems.Count);
        }

        // Generates JSON that represents an ItemSet in League
        public string GenerateJson()
        {
            var container = ApiTools.GetBlobContainer("itemsets");
            var blob = container.GetBlockBlobReference("ItemSetTemplate.txt");

            var json = blob.DownloadText();

            json = json.Replace("{Title}", "[LWI] " + Lane + " " + Champion.Name);

            json = json.Replace("{EarlyGameItems}", GenerateSectionJson(EarlyItems));
            json = json.Replace("{MidGameItems}", GenerateSectionJson(MidgameItems));
            json = json.Replace("{LateGameItems}", GenerateSectionJson(LategameItems));

            return json;

        }

        // Generates each item in JSON form and concatenates them together
        private string GenerateSectionJson(List<ItemStats> items)
        {
            var jsonItems = items.Select(item => "{ \"id\": \"" + item.ItemId + "\", \"count\": 1 }").ToList();

            return String.Join(",", jsonItems);
        }
    }
}