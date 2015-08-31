using System.Collections.Generic;

namespace LeagueStatisticallyBestItemset.Models
{
    public class FeaturedGames
    {
        public long ClientRefreshInterval { get; set; }
        public List<FeaturedGameInfo> GameList { get; set; } 
    }

    public class FeaturedGameInfo
    {
        public long GameId { get; set; }
        public long GameQueueConfigId { get; set; }
        public long MapId { get; set; }
    }
}