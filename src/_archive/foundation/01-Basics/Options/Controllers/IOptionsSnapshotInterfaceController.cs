using AspNetCore.OptionsPattern.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace AspNetCore.OptionsPattern.Controllers
{
    public class IOptionsSnapshotInterfaceController : Controller
    {
        private IOptionsSnapshot<PositionOptions> _optionsSnapshot;
        public IOptionsSnapshotInterfaceController(IOptionsSnapshot<PositionOptions> optionsSnapshot)
        {
            _optionsSnapshot = optionsSnapshot;
        }
        public IActionResult Index()
        {
            return View(_optionsSnapshot.Value);
        }
    }
}
