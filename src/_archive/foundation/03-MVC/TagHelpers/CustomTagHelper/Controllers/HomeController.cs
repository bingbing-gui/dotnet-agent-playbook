using AspNetCore.CustomTagHelpers.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Diagnostics;
using static AspNetCore.CustomTagHelpers.Models.Repository;

namespace AspNetCore.CustomTagHelpers.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        private readonly IRepository _repository;
        public HomeController(IRepository repository,
                              ILogger<HomeController> logger)

        {
            _repository = repository;
            _logger = logger;
        }
        public ViewResult Edit()
        {
            ViewBag.Quantity = new SelectList(_repository.Products.Select(p => p.Quantity).Distinct());
            return View("Create", _repository.Products.Last());
        }
        public IActionResult Index()
        {
            return View(_repository.Products);
        }
        public IActionResult Create()
        {
            ViewBag.Quantity = new SelectList(_repository.Products.Select(p => p.Quantity).Distinct());
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Product product)
        {
            _repository.AddProduct(product);
            return RedirectToAction("Index");
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