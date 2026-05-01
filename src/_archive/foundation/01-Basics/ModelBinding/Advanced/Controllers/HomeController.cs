using AspNetCore.ModelBinding.Advanced.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Reflection;

namespace AspNetCore.ModelBinding.Advanced.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private IRepository _repository;
        public HomeController(IRepository repository, ILogger<HomeController> logger)
        {
            _repository = repository;
            _logger = logger;
        }
        public IActionResult Index([FromQuery] int id = 1)
        {
            return View(_repository[id]);
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
        //public IActionResult Places(string[] places) => View(places);

        public IActionResult Places(List<string> places) => View(places);

        public IActionResult Address() => View();

        [HttpPost]
        public IActionResult Address(List<PersonAddress> address) => View(address);
        public IActionResult FromFormExample() => View();

        //[HttpPost]
        //public IActionResult FromFormExample([FromForm] Employee model)
        //{
        //    ViewBag.Message = "Employee data received";
        //    return View();
        //}

        [HttpPost]
        public Employee FromFormExample([FromForm] Employee model) => model;
        public IActionResult Body() => View();

        [HttpPost]
        public Employee Body([FromBody] Employee model) => model;

        public string Header([FromHeader(Name = "User-Agent")] string accept) => $"Header: {accept}";

        public IActionResult FullHeader(FullHeader model) => View(model);
        public IActionResult FromRouteExample() => View();
        [HttpPost]
        public string FromRouteExample([FromRoute] string id) => id;
    }
}