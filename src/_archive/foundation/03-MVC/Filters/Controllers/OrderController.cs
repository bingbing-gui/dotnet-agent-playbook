using AspNetCore.Filters.CustomFilters;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace AspNetCore.Filters.Controllers
{
    [ShowMessage("Controller", Order = 2)]
    public class OrderController : Controller
    {
        [ShowMessage("Action", Order = -1)]
        public IActionResult Index()
        {
            return View();

        }
    }
}
