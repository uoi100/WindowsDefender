using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace WindowsDefender_WebApp.Controllers
{
    public class MatchmakingController : Controller
    {
        
        [Authorize]
        public ActionResult Index()
        {
            return View();
        }

        [Authorize]
        public ActionResult Find()
        {
            return RedirectToAction("Lobby", "Matchmaking");
        }

        [Authorize]
        public ActionResult Lobby()
        {
            return View();
        }
    }
}