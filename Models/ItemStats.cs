using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;

namespace LeagueStatisticallyBestItemset.Models
{
    /// <summary>
    /// Container class that holds the Statistic information for an Item.
    /// Each Champion has indivually tracked Item stats based on Lane chosen.
    /// </summary>
    public class ItemStats
    {
        public int ItemId { get; set; }
        public int ChampionId { get; set; }
        public string Lane { get; set; }

        public float Uses { get; set; }
        public float Wins { get; set; }

        public float WinPercent
        {
            get { return Wins / Uses; }
        }

        public float AverageTimeBought { get; set; }

        /// <summary>
        /// Returns the Average time the Item was bought in the form Minutes : Seconds
        /// Ex: 4:23
        /// </summary>
        public string GetAverageTimeBought()
        {
            return TimeSpan.FromMilliseconds(AverageTimeBought).ToString(@"mm\:ss");
        }

        /// <summary>
        /// Returns the Section of the game the Item is bought in.
        /// Options are Early, Mid, and Late.
        /// The Section timings are configured in the Settings, and are compared to the Average Time using Less Than.
        /// </summary>
        /// <returns></returns>
        public Section GetTimeSection()
        {
            var time = TimeSpan.FromMilliseconds(AverageTimeBought);

            if (time < TimeSpan.FromMinutes(Convert.ToInt32(ConfigurationManager.AppSettings["EarlyGameLength"])))
                return Section.Early;
            
            if (time < TimeSpan.FromMinutes(Convert.ToInt32(ConfigurationManager.AppSettings["MidGameLength"])))
                return Section.Mid;

            return Section.Late;;
        }

        public enum Section
        {
            Early=0, Mid=1, Late=2
        }

        // Used for Debug only
        public override string ToString()
        {
            return "<br/>" +
            "Uses: " + Uses + "<br/>" +
            "Wins: " + Wins + "<br/>" +
            "Winrate: " + WinPercent * 100 + " %" +
            "<br/>" +
            "Average Time Bought: " + GetAverageTimeBought();
        }

        /// <summary>
        /// Generates a HSL color varying from red to green based on Winrate
        /// </summary>
        /// <returns>String of format hsl(X, Y%, Z%) with values for X Y and Z placed in</returns>
        public string GetBackgroundColor()
        {
            var h = (120 * WinPercent) + 10;
            var s = 100;
            var l = 60;

            return String.Format("hsl({0}, {1}%, {2}%)", h, s, l);
        }

        /// <summary>
        /// Returns the win percent
        /// </summary>
        /// <returns>String of format 85.25%</returns>
        public string GetWinPercent()
        {
            return Math.Round(WinPercent*100, 2) + " %";
        }
    }
}