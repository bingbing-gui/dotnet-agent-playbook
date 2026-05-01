using AspNetCore.DependencyInjection.Models;
using Microsoft.AspNetCore.Mvc;

namespace AspNetCore.DependencyInjection.Controllers
{
    public class HomeController : Controller
    {
        private IRepository _repository;
        public HomeController(IRepository repository)
        {
            _repository = repository;
        }
        public IActionResult Index([FromServices] ProductSum _productSum)
        {
            ViewBag.HomeControllerGUID = _repository.ToString();
            ViewBag.TotalGUID = _productSum.Repository.ToString();
            return View(_repository.Products);
        }
    }
}