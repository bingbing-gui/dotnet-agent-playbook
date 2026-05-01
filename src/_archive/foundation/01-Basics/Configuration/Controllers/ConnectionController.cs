using AspNetCore.Configuration.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace AspNetCore.Configuration.Controllers
{
    public class ConnectionController : Controller
    {
        private IOptions<Connections> _options;

        public ConnectionController(IOptions<Connections> options)
        {
            _options = options;
        }
        public IActionResult Index()
        {
            var connections = _options.Value.DefaultConnection;
            return View();
        }
    }
}
