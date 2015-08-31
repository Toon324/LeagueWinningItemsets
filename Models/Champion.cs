using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LeagueStatisticallyBestItemset.Models
{
    public class Champion
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Title { get; set; }
        public string Key { get; set; }

        public ImageData Image { get; set; }

        public override string ToString()
        {
            return Name + ", " + Title;
        }
    }
}