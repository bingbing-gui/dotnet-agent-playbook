using AspNetCore.OptionsPattern.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace AspNetCore.OptionsPattern.Controllers
{
    public class IOptionsInterfaceController : Controller
    {
        private IOptions<PositionOptions> _positionOptions;

        public IOptionsInterfaceController(IOptions<PositionOptions> options)
        {
            _positionOptions = options;
        }

        public IActionResult Index()
        {
            return View(_positionOptions.Value);
        }
    }
}
