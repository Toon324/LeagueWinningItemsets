using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using LeagueStatisticallyBestItemset.Services;

namespace LeagueStatisticallyBestItemset.Controllers
{
    public class RoleController : Controller
    {
        // GET: Lane
        public ActionResult Index(string id)
        {
            var champ = StaticDataService.GetChampion("na", Convert.ToInt32(id));

            return View(champ);
        }
    }
}