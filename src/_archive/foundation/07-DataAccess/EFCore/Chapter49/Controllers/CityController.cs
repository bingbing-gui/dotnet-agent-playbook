using EFCoreFluentAPIOneToMany.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace EFCoreFluentAPIOneToMany.Controllers
{
    public class CityController : Controller
    {
        private readonly CountryContext _countryContext;
        public CityController(CountryContext countryContext)
        {
            _countryContext = countryContext;
        }
        public IActionResult Index()
        {
            return View(_countryContext.City);
        }
        public IActionResult Create()
        {
            List<SelectListItem> country = new List<SelectListItem>();
            country = _countryContext.Country.Select(x => new SelectListItem { Text = x.Name, Value = x.Id.ToString() }).ToList();
            ViewBag.Country = country;
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Create(City city)
        {
            _countryContext.Add(city);
            await _countryContext.SaveChangesAsync();
            return RedirectToAction("Index");
        }
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var city = new City() { Id = id };
            _countryContext.Remove(city);
            await _countryContext.SaveChangesAsync();
            return RedirectToAction("Index");
        }

    }
}
