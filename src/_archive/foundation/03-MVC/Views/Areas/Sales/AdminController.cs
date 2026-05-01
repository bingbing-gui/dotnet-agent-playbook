using Microsoft.AspNetCore.Mvc;
namespace AspNetCore.Views.Areas.Sales
{
    public class AdminController : Controller
    {
        [Area("Sales")]
        public IActionResult List()
        {
            return View();
        }
    }
}
