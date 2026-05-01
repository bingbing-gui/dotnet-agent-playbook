using EFCoreFluentAPIOneToMany.Models;
using Microsoft.AspNetCore.Mvc;

namespace EFCoreFluentAPIOneToMany.Controllers
{
    public class CountryController : Controller
    {
        private readonly CountryContext _countryContext;

        public CountryController(CountryContext countryContext)
        {
            _countryContext = countryContext;
        }
        
        public IActionResult Index()
        {
            return View(_countryContext.Country);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(Country country)
        {
            _countryContext.Country.Add(country);
            await _countryContext.SaveChangesAsync();
            return RedirectToAction("Index");
        }
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var country = new Country() { Id = id };
            _countryContext.Remove(country);
            await _countryContext.SaveChangesAsync();
            return RedirectToAction("Index");
        }

    }
}
