using Microsoft.AspNetCore.Mvc;
namespace AspNetCore.Views.Controllers
{
    public class CityController : Controller
    {
        public IActionResult Tokyo()
        {
            return View();
        }
        [HttpPost]
        public IActionResult Tokyo(string name,string sex)
        {
            return View();
        }
        public IActionResult Nagoya()
        {
            return View();
        }
        public IActionResult LogcialOnView()
        {
            return View();
        }
        public IActionResult CallSharedView()
        {
            return View();
        }
    }
}