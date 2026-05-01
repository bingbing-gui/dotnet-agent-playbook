using Microsoft.AspNetCore.Mvc;

namespace AspNetCore.Views.Controllers
{
    public class AdminController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
        public IActionResult List()
        {
            return View();
        }
    }
}
