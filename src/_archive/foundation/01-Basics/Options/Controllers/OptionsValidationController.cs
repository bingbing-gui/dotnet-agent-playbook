using AspNetCore.OptionsPattern.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AspNetCore.OptionsPattern.Controllers
{
    public class OptionsValidationController : Controller
    {
        private readonly IOptions<MyConfigOptions> _config;
        private readonly ILogger<HomeController> _logger;
        public OptionsValidationController(IOptions<MyConfigOptions> config, 
                                           ILogger<HomeController> logger)
        {
            _config=config;
            _logger = logger;
            try
            {
                var configValue = _config.Value;

            }
            catch (OptionsValidationException ex)
            {
                foreach (var failure in ex.Failures)
                {
                    _logger.LogError(failure);
                }
            }
        }
        public IActionResult Index()
        {
            return View();
        }
    }
}
