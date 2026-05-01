using AspNetCore.Action.Models;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Diagnostics;

namespace AspNetCore.Action.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            //return RedirectToAction("Privacy");
            return RedirectToActionPermanent("Privacy");
        }
        public RedirectResult RedirectAction()
        {
            return Redirect("/List/Search");
        }
        public RedirectResult RedirectPermanentAction()
        {
            return RedirectPermanent("/List/Search");
        }
        public RedirectToRouteResult Redirect()
        {
            return RedirectToRoute(new { controller = "Admin", action = "Users", ID = 10 });
        }
        public RedirectToRouteResult RedirectPermanent()
        {
            return RedirectToRoutePermanent(new { controller = "Admin", action = "Users", ID = 10 });
        }
        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}