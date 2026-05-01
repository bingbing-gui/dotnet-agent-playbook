using EFCoreCodeFirst.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace EFCoreCodeFirst.Controllers
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

        public async Task<IActionResult> CreateInformation()
        {
            var info = new Information()
            {
                Name = "YogiHosting",
                License = "XXYY",
                Revenue = 1000,
                Establshied = Convert.ToDateTime("2014/06/24")
            };
            _companyContext.Entry(info).State = EntityState.Added;
            _companyContext.SaveChanges();
            return View();
        }

        public IActionResult Index()
        {
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