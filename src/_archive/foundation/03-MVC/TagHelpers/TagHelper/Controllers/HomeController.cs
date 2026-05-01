using AspNetCore.TagHelpers.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using static AspNetCore.TagHelpers.Models.Repository;
namespace AspNetCore.TagHelpers.Controllers
{
    public class HomeController : Controller
    {
        private IRepository _repository;

        private readonly ILogger<HomeController> _logger;

        public HomeController(IRepository repository,
            ILogger<HomeController> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View(_repository.Products);
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