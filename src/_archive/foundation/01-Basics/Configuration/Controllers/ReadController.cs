using AspNetCore.Configuration.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace AspNetCore.Configuration.Controllers
{
    public class ReadController : Controller
    {
        private readonly IOptions<MyWebApi> _options;
        public ReadController(IOptions<MyWebApi> options)
        {
            _options = options;
        }
        public IActionResult Index()
        {
            var apiName=_options.Value.Name;
            var url=_options.Value.Url;
            var secured=_options.Value.IsSecured;
            return View();
        }
    }
}
