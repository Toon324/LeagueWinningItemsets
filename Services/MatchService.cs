using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading;
using System.Web;
using LeagueStatisticallyBestItemset.Models;
using Newtonsoft.Json;
using RestSharp;

namespace LeagueStatisticallyBestItemset.Services
{
    public static class MatchService
    {
        private const string UrlFormat = "/api/lol/{region}/v2.2/match/{matchid}";

        /// <summary>
        /// Grabs the details of a Match from the Riot API
        /// </summary>
        /// <param name="region">Region to fetch from</param>
        /// <param name="matchId">Id of the Match to fetch</param>
        /// <returns>MatchDetail object containing the details of the match</returns>
        public static MatchDetail GetMatch(string region, int matchId)
        {
            Logger.LogMessageToFile(MethodBase.GetCurrentMethod().DeclaringType.ToString(), "Loading Match " + matchId);

            // Load the match from our Cache if we can, as this is much faster and saves on API calls
            var cachedMatch = CacheService<MatchDetail>.GetFromCache("MatchCache", region, matchId);
            if (cachedMatch != null && cachedMatch.MatchId != 0)
            {
                cachedMatch.FromCache = true;
                return cachedMatch;
            }

            var client = ApiTools.ApiClient(region);

            var request = new RestRequest(UrlFormat, Method.GET);

            request.AddUrlSegment("region", region);
            request.AddUrlSegment("matchid", matchId.ToString());
            request.AddParameter("includeTimeline", true);

            request.AddApiKey();

            var response = client.Execute<MatchDetail>(request);

            //Check to see if we are approaching rate limiting
            if (response.StatusCode == HttpStatusCode.ServiceUnavailable || response.StatusCode.ToString() == "429")
            {
                Logger.LogMessageToFile(MethodBase.GetCurrentMethod().DeclaringType.ToString(), "Too many calls, briefly pausing. Headers: " 
                    + String.Join(",", response.Headers));
                Thread.Sleep(Convert.ToInt32(ConfigurationManager.AppSettings["msBetweenApiCalls"])*2);
            }

            var match = response.Data;
            match.FromCache = false;

            Logger.LogMessageToFile(MethodBase.GetCurrentMethod().DeclaringType.ToString(), "ResponseCode: " + response.StatusCode);

            if (match.MatchId == 0)
                Logger.LogMessageToFile(MethodBase.GetCurrentMethod().DeclaringType.ToString(), "Warning: Did not correctly load Match " + 
                    matchId + " Response: " + response.StatusDescription);
            else
                CacheService<MatchDetail>.WriteToCache("MatchCache", region, matchId, match);
                // Save match to file cache
                

            return match;
        }

        /// <summary>
        /// Loads a series of Matches from the Riot API
        /// </summary>
        /// <param name="region">Region to fetch from </param>
        /// <param name="matchIds">List of Match Id's to load</param>
        /// <returns>List of MatchDetail objects </returns>
        public static List<MatchDetail> GetMatchesFromList(string region, List<int> matchIds)
        {
            var totalIds = matchIds.Count();

            Logger.LogMessageToFile(MethodBase.GetCurrentMethod().DeclaringType.ToString(), "Loading " + totalIds + " matches");

            var matches = new List<MatchDetail>();

            var completed = 0;

            foreach (var matchId in matchIds)
            {
                var match = GetMatch(region, matchId);
                matches.Add(match);

                // Keep track of progress
                completed++;
                if (completed % 10 == 0)
                    Logger.LogMessageToFile(MethodBase.GetCurrentMethod().DeclaringType.ToString(), "Progress: " 
                        + completed + "/" + totalIds + "   " +
                        completed.PercentOf(totalIds) + "%");

                // We can only load 500 requests every 10 minutes, so we slow down method execution
                if (!match.FromCache)
                    Thread.Sleep(Convert.ToInt32(ConfigurationManager.AppSettings["msBetweenApiCalls"]));
            }

            return matches.Where(match => match.MatchId != 0).ToList();
        }

        /// <summary>
        /// Scrapes the Id's of current featured Matches in a region. Used to keep statistics up to date.
        /// This method is called externally.
        /// </summary>
        /// <param name="region"></param>
        public static void ScrapeCurrentFeaturedGames(string region)
        {
            Logger.LogMessageToFile(MethodBase.GetCurrentMethod().DeclaringType.ToString(), "Scraping current featured games");
            var client = ApiTools.ApiClient(region);
            var request =
                new RestRequest(
                    "/observer-mode/rest/featured",
                    Method.GET);

            request.AddApiKey();

            var response = client.Execute<FeaturedGames>(request);

            //Check to see if we are approaching rate limiting
            if (response.StatusCode == HttpStatusCode.ServiceUnavailable || response.StatusCode.ToString() == "429")
            {
                Logger.LogMessageToFile(MethodBase.GetCurrentMethod().DeclaringType.ToString(), "Too many calls, briefly pausing. Headers: "
                    + String.Join(",", response.Headers));
                Thread.Sleep(Convert.ToInt32(ConfigurationManager.AppSettings["msBetweenApiCalls"]) * 2);
            }

            var container = ApiTools.GetBlobContainer("matches");
            var blob = container.GetAppendBlobReference("matchIds.txt");

            if (!blob.Exists())
                blob.CreateOrReplace();

            foreach (var game in response.Data.GameList)
            {
                // Only scrape Ranked games on Summoner's Rift
                if ((game.GameQueueConfigId == 4 || game.GameQueueConfigId == 42) && game.MapId == 11)
                {
                    blob.AppendText(game.GameId.ToString() + "\n");
                }
            }

            Logger.LogMessageToFile(MethodBase.GetCurrentMethod().DeclaringType.ToString(), String.Format("Scrape complete, {0} games scraped", response.Data.GameList.Count));
        }
    }
}