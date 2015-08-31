using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Caching;
using System.Web.Helpers;
using LeagueStatisticallyBestItemset.Models;
using RestSharp;

namespace LeagueStatisticallyBestItemset.Services
{
    public static class StaticDataService
    {
        private const string UrlFormat = "/api/lol/static-data/{region}/v1.2/{method}";

        /// <summary>
        /// Fetches the URL used to grab CDN assets from DataDragon
        /// </summary>
        /// <param name="region">Region of data</param>
        /// <returns>DataDragon object containing data on the current Version of DataDragon</returns>
        public static DataDragon GetCdnUrl(string region)
        {
            var cacheName = "cdn_" + region;
            var cached = HttpContext.Current.Cache[cacheName] as DataDragon;

            if (cached != null)
                return cached;

            var client = ApiTools.GlobalApiClient();

            var request = new RestRequest(UrlFormat, Method.GET);

            request.AddUrlSegment("region", region);
            request.AddUrlSegment("method", "realm");

            request.AddApiKey();

            var response = client.Execute<DataDragon>(request);

            HttpContext.Current.Cache[cacheName] = response.Data;

            return response.Data;
        }

        /// <summary>
        /// Loads an Item from cache or from the API
        /// </summary>
        /// <param name="region">Region of data</param>
        /// <param name="itemId">ID of item</param>
        public static Item GetItem(string region, long itemId)
        {
            // Two layer caching: File and CurrentCache.
            var cacheName = "item_" + region + "_" + itemId;
            var cached = HttpContext.Current.Cache[cacheName] as Item;

            if (cached != null)
                return cached;

            cached = CacheService<Item>.GetFromCache("ItemCache", region, (int)itemId);

            if (cached != null)
                return cached;

            var client = ApiTools.ApiClient(region);

            var request = new RestRequest(UrlFormat + "/{itemid}", Method.GET);

            request.AddUrlSegment("region", region);
            request.AddUrlSegment("method", "item");
            request.AddUrlSegment("itemid", itemId.ToString());

            request.AddParameter("itemData", "consumed,from,gold,into,image");

            request.AddApiKey();

            var response = client.Execute<Item>(request);

            // Write to both File and Current cache
            CacheService<Item>.WriteToCache("ItemCache", region, (int)itemId, response.Data);
            HttpContext.Current.Cache[cacheName] = response.Data;

            return response.Data;
        }

        /// <summary>
        /// Loads an Champion from cache or from the API
        /// </summary>
        /// <param name="region">Region of data</param>
        /// <param name="championId">ID of champion</param>
        public static Champion GetChampion(string region, int championId)
        {
            // Two layer caching: File and CurrentCache.
            var cacheName = "champ_" + region + "_" + championId;
            var cached = HttpContext.Current.Cache[cacheName] as Champion;

            if (cached != null)
                return cached;

            cached = CacheService<Champion>.GetFromCache("ChampionCache", region, championId);

            if (cached != null)
                return cached;

            var client = ApiTools.ApiClient(region);

            var request = new RestRequest(UrlFormat + "/{championid}", Method.GET);

            request.AddUrlSegment("region", region);
            request.AddUrlSegment("method", "champion");
            request.AddUrlSegment("championid", championId.ToString());

            request.AddParameter("champData", "image");

            request.AddApiKey();

            var response = client.Execute<Champion>(request);

            // Write to both File and Current cache
            CacheService<Champion>.WriteToCache("ChampionCache", region, championId, response.Data);
            HttpContext.Current.Cache[cacheName] = response.Data;

            return response.Data;
        }

        /// <summary>
        /// Returns a list of all Champions in the current Version
        /// </summary>
        /// <param name="region">Region of data</param>
        public static List<Champion> GetAllChampions(string region)
        {
            var cacheName = "allChamps_" + region;
            var cached = HttpContext.Current.Cache[cacheName] as List<Champion>;

            if (cached != null)
                return cached;

            var client = ApiTools.GlobalApiClient();

            var request = new RestRequest(UrlFormat, Method.GET);

            request.AddUrlSegment("region", region);
            request.AddUrlSegment("method", "champion");

            request.AddParameter("dataById", true);

            request.AddApiKey();

            var response = client.Execute(request);
            var response2 = client.Execute<dynamic>(request);

            var champs = ParseAllChampsResponse(region, response.Content);

            HttpContext.Current.Cache[cacheName] = champs;

            return champs;
        }

        /// <summary>
        /// The API response for All Champions is in a format that is difficult to automatically parse, so this method manually parses it.
        /// </summary>
        /// <param name="region">Region of data</param>
        /// <param name="response">Response to parse</param>
        /// <returns>List of Champion from a Response</returns>
        private static List<Champion> ParseAllChampsResponse(string region, string response)
        {
            try
            {
                dynamic data = Json.Decode(response);

                var ids = new List<int>();
                foreach (var segment in data.data)
                {
                    ids.Add(Convert.ToInt32(segment.Key));
                }
                return ids.Select(id => GetChampion(region, id)).ToList();
            }
            catch
            {
                return new List<Champion>();
            }
        } 
    }
}