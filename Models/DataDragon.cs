using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using LeagueStatisticallyBestItemset.Services;

namespace LeagueStatisticallyBestItemset.Models
{
    [DataContract]
    public class DataDragon
    {
        [DataMember(Name = "v")]
        public string Version { get; set; }

        [DataMember(Name = "cdn")]
        public string BaseUrl { get; set; }

        public string GetCdnUrl()
        {
            return BaseUrl + "/" + Version;
        }
    } 

    public class ImageData
    {
        private const string BaseUrl = "http://ddragon.leagueoflegends.com/cdn";
        private const string VersionUrl = "/5.16.1";
        private const string ItemUrl = "/img/item/";
        private const string ChampionSquareUrl = "/img/champion/";
        private const string ChampionLoadingUrl = "/img/champion/loading/";

        public string Full { get; set; }
        public string Group { get; set; }
        public string Sprite { get; set; }

        public int W { get; set; }
        public int H { get; set; }

        public enum ImageTypes
        {
            Splash, Loading, Square, Item
        }

        public string GetImageUrl(string region, ImageTypes typeToLoad)
        {
            switch (typeToLoad)
            {
                case ImageTypes.Splash:
                    return BaseUrl + "/img/champion/splash/" + Full.Replace(".png", "_0.jpg");
                case ImageTypes.Item:
                    return BaseUrl + VersionUrl + "/img/item/" + Full;
                case ImageTypes.Loading:
                    return BaseUrl + "/img/champion/loading/" + Full.Replace(".png", "_0.jpg");
                case ImageTypes.Square:
                    return BaseUrl + VersionUrl + "/img/champion/" + Full;
            }
            return Group == "item" ? ItemUrl + Full : ChampionSquareUrl + Full;
        }
    }
}