using Microsoft.AspNetCore.Mvc;

namespace AspNetCore.Action.Controllers
{
    public class ListController : Controller
    {
        public IActionResult Search()
        {
            return View();
        }

    }
}
