using System;
using System.Collections.Generic;
using System.Configuration;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using RestSharp;

namespace LeagueStatisticallyBestItemset.Services
{
    public static class ApiTools
    {
        /// <summary>
        /// A list of all possible Roles
        /// </summary>
        public static List<String> GetRoles()
        {
            return new List<string> { "top", "jungle", "middle", "bottom" };
        }

        /// <summary>
        /// Returns a float number indicating the percent some int is of another int.
        /// Called as x.PercentOf(y).
        /// Equivalant of (x / y) * 100
        /// </summary>
        /// <param name="upper">The top value x</param>
        /// <param name="lower">The bottom value y</param>
        public static float PercentOf(this int upper, int lower)
        {
            return ((float) upper/lower)*100;
        }

        /// <summary>
        /// Used for Request URL formatting.
        /// Replaces all instances of {region} with given string
        /// Example:
        /// var url = www.api.com/{region}/help;
        /// var requstUrl = url.AddRegion("NA");
        /// 
        /// requestUrl would be "www.api.com/NA/help"
        /// </summary>
        /// <param name="url">Base string</param>
        /// <param name="region">String to replace {region} with</param>
        /// <returns></returns>
        public static string AddRegion(this string url, string region)
        {
            return url.Replace("{region}", region);
        }

        /// <summary>
        /// Adds the Riot API key to a Request
        /// </summary>
        /// <param name="request">RestRequest to add API key to</param>
        public static void AddApiKey(this RestRequest request)
        {
            request.AddParameter("api_key", ConfigurationManager.AppSettings["ApiKey"]);
        }

        /// <summary>
        /// Returns the Base URL that API calls are made to
        /// </summary>
        /// <param name="region">Region to pass to URL</param>
        /// <returns>URL such as "https://na.api.pvp.net"</returns>
        public static string ApiBaseUrl(string region)
        {
            return "https://{region}.api.pvp.net".AddRegion(region);
        }

        /// <summary>
        /// Returns a RestClient that gives access to the Global API (https://global.api.pvp.net)
        /// </summary>
        public static RestClient GlobalApiClient()
        {
            return new RestClient("https://global.api.pvp.net");
        }

        /// <summary>
        /// Returns a RestClient that gives access to a Regional API (https://{region}.api.pvp.net)
        /// </summary>
        /// <param name="region">Region of API</param>
        public static RestClient ApiClient(string region)
        {
            return new RestClient(ApiBaseUrl(region));
        }

        /// <summary>
        /// Returns an Azure blob Container
        /// </summary>
        /// <param name="cache">Name of container</param>
        public static CloudBlobContainer GetBlobContainer(string cache)
        {
            var account =
                CloudStorageAccount.Parse(ConfigurationManager.ConnectionStrings["AzureWebJobsStorage"].ConnectionString);

            var client = account.CreateCloudBlobClient();

            var blobContainer = client.GetContainerReference(cache.ToLower());

            blobContainer.CreateIfNotExists();

            return blobContainer;
        }
    }
}