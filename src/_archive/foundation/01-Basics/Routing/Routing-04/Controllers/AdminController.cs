using Microsoft.AspNetCore.Mvc;

namespace AspNetCore.RouteLinks.Controllers
{
    public class AdminController : Controller
    {
        [Route("News/[controller]/USA/[action]/{id?}")]
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }


    }
}
