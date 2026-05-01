using EFCoreDbContext.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace EFCoreDbContext.Controllers
{
    public class HomeController : Controller
    {

        private readonly CompanyContext _companyContext;
        private readonly ILogger<HomeController> _logger;

        public HomeController(CompanyContext companyContext, ILogger<HomeController> logger)
        {
            _companyContext = companyContext;
            _logger = logger;
        }

        public IActionResult Index()
        {
            var dept = new Department()
            {
                Name = "Designing"
            };
            _companyContext.Entry(dept).State=EntityState.Added;
            _companyContext.SaveChanges();
            return View();
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