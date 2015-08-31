using System;
using System.Collections.Generic;
using Microsoft.WindowsAzure.Storage.Blob;
using Newtonsoft.Json;

namespace LeagueStatisticallyBestItemset.Services
{
    /// <summary>
    /// Class that accesses Azure Web Blob storage as a file-based cache for data.
    /// </summary>
    /// <typeparam name="T">Type of Object to load or store</typeparam>
    public static class CacheService<T>
    {
        /// <summary>
        /// Returns an Object from the cache
        /// </summary>
        /// <param name="cache">Name of Cache to load form</param>
        /// <param name="region">Region of data</param>
        /// <param name="id">ID of data</param>
        /// <returns></returns>
        public static T GetFromCache(string cache, string region, int id)
        {
            return GetFromCache(cache, region + "_" + id + ".json");
        }

        /// <summary>
        /// Returns an Object from the cache
        /// Overloaded method that allows custom Data names to be passed in
        /// </summary>
        /// <param name="cache">Name of Cache to load from</param>
        /// <param name="filename">Name of Data Blob</param>
        /// <returns></returns>
        public static T GetFromCache(string cache, string filename)
        {
            try
            {
                var container = ApiTools.GetBlobContainer(cache);

                var blob = container.GetBlockBlobReference(filename);

                if (!blob.Exists())
                    return default(T);

                var json = blob.DownloadText();
                var toReturn = JsonConvert.DeserializeObject<T>(json);

                return toReturn;

            }
            catch (Exception e)
            {
                Logger.LogMessageToFile("CacheService [" + cache + "]", e.Message);
                return default(T);
            }
        }

        /// <summary>
        /// Returns all Blob items from the Cache
        /// </summary>
        /// <param name="cache">Name of Cache to load from</param>
        /// <returns></returns>
        public static List<T> GetAllFromCache(string cache)
        {
            var container = ApiTools.GetBlobContainer(cache);

            var allBlobs = container.ListBlobs(null, true);

            var toReturn = new List<T>();

            foreach (var blob in allBlobs)
            {
                var temp = (CloudBlockBlob)blob;
                toReturn.Add(JsonConvert.DeserializeObject<T>(temp.DownloadText()));
            }

            return toReturn;
        }

        /// <summary>
        /// Writes an Object to a Cache
        /// </summary>
        /// <param name="cache">Name of Cache to write to</param>
        /// <param name="region">Region of data</param>
        /// <param name="id">ID of data</param>
        /// <param name="toCache">Object to cache</param>
        /// <returns></returns>
        public static bool WriteToCache(string cache, string region, int id, T toCache)
        {
            return WriteToCache(cache, region, id.ToString(), toCache);
        }

        /// <summary>
        /// Writes an Object to a Cache
        /// </summary>
        /// <param name="cache">Name of Cache to write to</param>
        /// <param name="region">Region of data</param>
        /// <param name="id">ID of data</param>
        /// <param name="toCache">Object to cache</param>
        /// <returns></returns>
        public static bool WriteToCache(string cache, string region, string id, T toCache)
        {
            var json = JsonConvert.SerializeObject(toCache);

            return WriteToCache(cache, region, id, json);
        }

        /// <summary>
        /// Writes a Jsonified Object to a Cache
        /// </summary>
        /// <param name="cache">Name of Cache to write to</param>
        /// <param name="region">Region of data</param>
        /// <param name="id">ID of data</param>
        /// <param name="json">JSON to write</param>
        /// <returns></returns>
        public static bool WriteToCache(string cache, string region, string id, string json)
        {
            try
            {
                var container = ApiTools.GetBlobContainer(cache);

                var blob = container.GetBlockBlobReference(region + "_" + id + ".json");

                blob.UploadText(json);

                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Returns all Blobs in a Cache that match based on a certain Prefix
        /// </summary>
        /// <param name="cache">Cache to load from</param>
        /// <param name="region">Region of Data</param>
        /// <param name="champId">ChampId of Data</param>
        /// <param name="role">Role of Data</param>
        /// <returns></returns>
        public static List<T> GetListFromCache(string cache, string region, int champId, string role)
        {

            var container = ApiTools.GetBlobContainer(cache);

            var allBlobs = container.ListBlobs(region + "_" + champId + "_" + role, true);


            var toReturn = new List<T>();

            foreach (var blob in allBlobs)
            {
                var temp = (CloudBlockBlob)blob;
                toReturn.Add(JsonConvert.DeserializeObject<T>(temp.DownloadText()));
            }

            return toReturn;
        }
    }
}