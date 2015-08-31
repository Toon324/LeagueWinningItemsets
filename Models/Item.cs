using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LeagueStatisticallyBestItemset.Models
{
    /// <summary>
    /// Represents an In-Game item
    /// </summary>
    public class Item
    {
        /// <summary>
        /// Indicates if the item can be consumed (Health Pots are included, Crystalline Flask is not)
        /// </summary>
        public bool Consumed { get; set; }

        public string Name { get; set; }
        public ItemGold Gold { get; set; }
        public int Id { get; set; }

        /// <summary>
        /// The Id's of any Items this Item builds into
        /// </summary>
        public List<string> Into { get; set; }

        /// <summary>
        /// The Id's of any Items this Item builds from
        /// </summary>
        public List<string> From { get; set; }

        public ImageData Image { get; set; }

        /// <summary>
        /// Returns true if this Item does not build into any other Items
        /// </summary>
        public bool IsFinalItem
        {
            get { return Into == null || !Into.Any(); }
        }

        // Used for Debug only
        public override string ToString()
        {
            var toReturn = "<div>" +
                Id + " : " + 
                Name + "<br/>" +
                "Consumable? " + Consumed +
                Gold;

            if (Into != null && Into.Any())
                toReturn += "Builds into " + Into.Count() + " items. <br />";
            if (From != null && From.Any())
                toReturn += "Builds from " + From.Count() + " items.";

            toReturn += "</div>";

            return toReturn;
        }
    }

    /// <summary>
    /// Represents the amount of Gold needed to buy this Item, and the total amount of Gold needed to get to this Item.
    /// </summary>
    public class ItemGold
    {
        public int Base { get; set; }
        public int Total { get; set; }

        public override string ToString()
        {
            return "<br /> Base Price: " + Base + " <br /> " + "Total Price: " + Total + "<br />";
        }
    }
}