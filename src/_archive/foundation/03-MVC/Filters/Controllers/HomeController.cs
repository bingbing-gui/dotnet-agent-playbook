using AspNetCore.Filters.CustomFilters;
using AspNetCore.Filters.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Diagnostics;

namespace AspNetCore.Filters.Controllers
{
    //[RequireHttps]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }
        [HttpsOnly]
        [TimeElapsedAsync]
        public string Index()
        {
            return "This is the Index action on the Home controller";
        }
        //[ChangeView]
        [ChangeViewAsync]
        public IActionResult Message()
        {
            return View();
        }
        [HybridActRes]
        public IActionResult List()
        {
            return View();
        }
        //[CatchError]
        [TypeFilter(typeof(CatchErrorMessage))]
        public IActionResult Exception(int? id)
        {
            if (id == null)
                throw new Exception("Error Id cannot be null");
            else
                return View((Object)$"The value is {id}");
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