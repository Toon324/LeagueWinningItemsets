using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using LeagueStatisticallyBestItemset.Services;

namespace LeagueStatisticallyBestItemset.Models
{
    public class Participant
    {
        public int ChampionId { get; set; }
        public int ParticipantId { get; set; }
        public int TeamId { get; set; }

        public ParticipantStats Stats { get; set; }
        public ParticipantTimeline Timeline { get; set; }

        public List<Item> GetFinalItems()
        {
            return Stats.FinalItemIds.Select(itemId => StaticDataService.GetItem("na", itemId)).ToList();
        }
    }

    public class ParticipantStats
    {
        public long Item0 { get; set; }
        public long Item1 { get; set; }
        public long Item2 { get; set; }
        public long Item3 { get; set; }
        public long Item4 { get; set; }
        public long Item5 { get; set; }
        public long Item6 { get; set; }

        public long Kills { get; set; }
        public long Deaths { get; set; }
        public long Assists { get; set; }

        public List<long> FinalItemIds
        {
            get { return new List<long> { Item0, Item1, Item2, Item3, Item4, Item5, Item6 }; }
        }
    }

    public class ParticipantTimeline
    {
        public string Lane { get; set; }
        public string Role { get; set; }
    }
}