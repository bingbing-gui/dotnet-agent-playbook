using AspNetCore.URLRouting.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace AspNetCore.URLRouting.Controllers
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
            return View();
        }

        public IActionResult Old()
        {
            return View();
        }

        //public IActionResult Check()
        //{
        //    ViewBag.ValueofId = RouteData.Values["id"];
        //    return View();
        //}
        //public IActionResult Check(int id)
        //{
        //    ViewBag.ValueofId = id;
        //    return View();
        //}
        //public IActionResult Check(string id)
        //{
        //    ViewBag.ValueofId = id;
        //    return View();
        //}
        public IActionResult Check(string id)
        {
            ViewBag.ValueofId = id ?? "Null Value";
            return View();
        }
        public IActionResult CatchallTest(string id, string catchall)
        {
            ViewBag.ValueofId = id;
            ViewBag.ValueofCatchall = catchall;
            return View();
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