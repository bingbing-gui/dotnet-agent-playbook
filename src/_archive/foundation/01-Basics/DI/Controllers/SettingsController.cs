using AspNetCore.DependencyInjection.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace AspNetCore.DependencyInjection.Controllers
{
    public class SettingsController : Controller
    {
        private readonly MyJson _settings;
        public SettingsController(IOptions<MyJson> options)
        {
            _settings = options.Value;
        }
        public IActionResult Index()
        {
            ViewData["Title"] = _settings.Title;
            ViewData["Version"] = _settings.Version;
            return View();
        }
        public IActionResult Show()
        {
            return View();
        }
        public IActionResult Detail()
        {
            return View();
        }
    }
}
