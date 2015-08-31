using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Linq;
using System.Web;

namespace LeagueStatisticallyBestItemset.Models
{
    public class MatchDetail
    {
        public bool FromCache { get; set; }

        public int MapId { get; set; }
        public int MatchId { get; set; }

        public string MatchMode { get; set; }
        public string MatchType { get; set; }
        public string Region { get; set; }

        public List<Team> Teams { get; set; } 
        public List<Participant> Participants { get; set; }
        public Timeline Timeline { get; set; }

        public List<ItemPurchase> GetItemPurchases(Participant participant)
        {
            var toReturn = new List<ItemPurchase>();

            foreach (var frame in Timeline.Frames.Where(frame => frame.Events != null))
            {
                var purchaseEvents = frame.Events.Where(
                    x => !String.IsNullOrEmpty(x.EventType) &&
                    x.EventType == "ITEM_PURCHASED" &&
                    x.ParticipantId == participant.ParticipantId);

                toReturn.AddRange(purchaseEvents.Select(purchase => new ItemPurchase(frame.Timestamp, purchase.ItemId, purchase.ParticipantId)));
            }

            return toReturn;
        }

        public List<ItemPurchase> GetAllItemPurchases()
        {
            var toReturn = new List<ItemPurchase>();

            foreach (var frame in Timeline.Frames.Where(frame => frame.Events != null))
            {
                var purchaseEvents = frame.Events.Where(
                    x => !String.IsNullOrEmpty(x.EventType) &&
                    x.EventType == "ITEM_PURCHASED");

                toReturn.AddRange(purchaseEvents.Select(purchase => new ItemPurchase(frame.Timestamp, purchase.ItemId, purchase.ParticipantId)));
            }

            return toReturn;
        } 

        public bool ParticipantWon(Participant participant)
        {
            return Teams.First(team => team.Winner).TeamId == participant.TeamId;
        }
    }

    public class Team
    {
        public int TeamId { get; set; }
        public bool Winner { get; set; }
    }

    public class Timeline
    {
        public List<Frame> Frames { get; set; }
    }

    public class Frame
    {
        public long Timestamp { get; set; }
        public List<Event> Events { get; set; }
    }

    public class Event
    {
        public string EventType { get; set; }
        public int ItemId { get; set; }
        public int ParticipantId { get; set; }
    }

    public class ItemPurchase
    {
        public ItemPurchase(long timestamp, int itemId, int participantId)
        {
            Timestamp = timestamp;
            ItemId = itemId;
            ParticipantId = participantId;
        }

        public long Timestamp { get; set; }
        public int ItemId { get; set; }
        public int ParticipantId { get; set; }
    }
}