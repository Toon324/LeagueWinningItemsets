using System.Linq;
using System.Web.Mvc;
using LeagueStatisticallyBestItemset.Services;

namespace LeagueStatisticallyBestItemset.Controllers
{
    public class HomeController : Controller
    {
        private const string Region = "na";

        public ActionResult Index()
        {
            var champs = StaticDataService.GetAllChampions("na");

            return View(champs);
        }

        public ActionResult Generate()
        {
            var itemSetService = new ItemSetService();
            var matchset =
                itemSetService.LoadMatchsetFromFile("NA.json");

            var matches = MatchService.GetMatchesFromList(Region, matchset.ToList());

            itemSetService.GenerateStatsFromMatchset(Region, matches);

            itemSetService.WriteStatsToFile(Region);

            var itemStats = itemSetService.GetAllItemStats();

            return View(itemStats);
        }

        public ActionResult Scrape()
        {
            MatchService.ScrapeCurrentFeaturedGames("na");

            return Content("Success");
        }
    }
}
