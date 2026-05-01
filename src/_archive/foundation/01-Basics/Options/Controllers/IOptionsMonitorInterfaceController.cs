using AspNetCore.OptionsPattern.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace AspNetCore.OptionsPattern.Controllers
{
    public class IOptionsMonitorInterfaceController : Controller
    {
        private IOptionsMonitor<PositionOptions> _positionOptions;
        public IOptionsMonitorInterfaceController(IOptionsMonitor<PositionOptions> optionsMonitor)
        {
            _positionOptions = optionsMonitor;
        }
        public IActionResult Index()
        {
            return View(_positionOptions.CurrentValue);
        }
    }
}
