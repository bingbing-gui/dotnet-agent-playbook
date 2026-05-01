using Microsoft.AspNetCore.Mvc;

namespace AspNetCore.Action.Controllers
{
    public class AdminController : Controller
    {
        public IActionResult Users(int id)
        {
            ViewBag.Id = id;
            return View();
        }
    }
}
