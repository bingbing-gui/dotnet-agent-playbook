using AspNetCore.ModelBinding.Models;
using Microsoft.AspNetCore.Mvc;
namespace AspNetCore.ModelBinding.Controllers
{
    public class HomeController : Controller
    {
        private IRepository repository;
        public HomeController(IRepository repo)
        {
            repository = repo;
        }
        public IActionResult Index(int id = 1)
        {
            return View(repository[id]);
        }
        public IActionResult Create() => View();
        [HttpPost]
        public IActionResult Create(Employee model) => View("Index", model);
        //[HttpPost]
        //public IActionResult DisplayPerson([Bind(Prefix = nameof(Employee.HomeAddress))] PersonAddress personAddress)
        //{
        //    return View(personAddress);
        //}
        [HttpPost]
        public IActionResult DisplayPerson([Bind(nameof(PersonAddress.City), Prefix = nameof(Employee.HomeAddress))] PersonAddress personAddress)
        {
            return View(personAddress);
        }
    }
}