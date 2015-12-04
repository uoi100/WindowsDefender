using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace WindowsDefender_WebApp.Controllers {
    public class HomeController : Controller {

        public ActionResult Index() {
            if (User.Identity.IsAuthenticated)
                return RedirectToAction("Index", "Matchmaking");
            return View();
        }

        public ActionResult About() {
            ViewBag.Message = "Your application description page.";
            return View();
        }

        public ActionResult Download()
        {
            return View();
        }

        public ActionResult Guide()
        {
            return View();
        }

        public ActionResult Contact() {
            ViewBag.Message = "Your contact page.";
            return View();
        }
    }
}