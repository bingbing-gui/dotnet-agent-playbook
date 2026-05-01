using AspNetCore.OptionsPattern.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using System.Diagnostics;

namespace AspNetCore.OptionsPattern.Controllers
{
    public class HomeController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<HomeController> _logger;
        public HomeController(IConfiguration configuration,
                              ILogger<HomeController> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }
        public IActionResult Index()
        {
            return View();
        }
        public IActionResult Bind()
        {
            #region Bind 方法
            var positionOptions = new PositionOptions();
            _configuration.GetSection(PositionOptions.Position).Bind(positionOptions);
            #endregion
            return View(positionOptions);
        }
        public IActionResult GetOfT()
        {
            #region Get 方法
            var positionOptions = _configuration.GetSection(PositionOptions.Position)
                                                     .Get<PositionOptions>();
            #endregion
            return View(positionOptions);
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