using AspNetCore.GlobalLocalResFiles.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using System.Diagnostics;

namespace AspNetCore.GlobalLocalResFiles.Controllers
{
    public class HomeController : Controller
    {
        private readonly IStringLocalizer<HomeController> _localizer;

        public HomeController(IStringLocalizer<HomeController> localizer)
        {
            _localizer = localizer;
        }
        public IActionResult Index()
        {
            return View();
        }
        [HttpPost]
        public IActionResult Index(JobApplication jobApplication)
        {
            if (ModelState.IsValid)
                ViewBag.Message = _localizer["Your application is accepted"];
            return View();
        }
    }
}