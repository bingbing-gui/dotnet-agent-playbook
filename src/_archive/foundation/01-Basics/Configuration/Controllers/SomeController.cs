using Microsoft.AspNetCore.Mvc;

namespace AspNetCore.Configuration.Controllers
{
    public class SomeController : Controller
    {
        private IWebHostEnvironment _env;

        private IConfiguration _config;
        public SomeController(IWebHostEnvironment hostingEnvironment, IConfiguration configuration)
        {
            _env = hostingEnvironment;
            _config = configuration;
        }
        public IActionResult Index()
        {
            bool contentMiddleware = Convert.ToBoolean(_config["Middleware:EnableContentMiddleware"]);
            if (_env.IsDevelopment())
            {

            }
            if (_env.IsStaging())
            {

            }
            if (_env.IsProduction())
            {

            }
            return View();
        }
    }
}
