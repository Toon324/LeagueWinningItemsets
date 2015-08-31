using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using LeagueStatisticallyBestItemset.Services;

namespace LeagueStatisticallyBestItemset.Controllers
{
    public class StatsController : Controller
    {
        public ActionResult Index(int champId, string role)
        {
            try
            {
                var itemSetService = new ItemSetService();

                var itemset = itemSetService.GetItemset("na", champId, role);

                if (itemset == null
                    || itemset.Champion == null
                    || itemset.EarlyItems.Count <= 2
                    || itemset.MidgameItems.Count < 2 
                    || itemset.LategameItems.Count < 2)
                {
                    return View("NotEnoughInfo");
                }
                return View(itemset);
            }
            catch (Exception e)
            {
                return View("NotEnoughInfo");
            }
        }

        /// <summary>
        /// Returns the JSON file of the Itemset, for use in League.
        /// </summary>
        public ActionResult Itemset(int champId, string role)
        {
            try
            {
                var itemSetService = new ItemSetService();
                var itemSet = itemSetService.GetItemsetBlob("na", champId, role);

                return Redirect(itemSet.Uri.AbsoluteUri);
            }
            catch (Exception e)
            {
                return Content(e.Message);
            }
        }
    }
}