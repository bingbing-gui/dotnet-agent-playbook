using Microsoft.AspNetCore.Mvc;
namespace AspNetCore.Action.Controllers
{
    public class ExampleController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
        public IActionResult ViewBagExample()
        {
            ViewBag.CurrentDateTime = DateTime.Now;
            ViewBag.CurrentYear = DateTime.Now.Year;
            return View();
        }
        public IActionResult TempDataExample()
        {
            TempData["CurrentDateTime"] = DateTime.Now;
            TempData["CurrentYear"] = DateTime.Now.Year;
            return RedirectToAction("TempDataShow");
        }
        public IActionResult TempDataShow()
        {
            return View();
        }
        public JsonResult ReturnJson()
        {
            return Json(new[] { "Brahma", "Vishnu", "Mahesh" });
        }

        public StatusCodeResult ReturnBadRequest()
        {
            return StatusCode(StatusCodes.Status400BadRequest);
        }
        public StatusCodeResult ReturnUnauthorized()
        {
            return StatusCode(StatusCodes.Status401Unauthorized);
        }
        public StatusCodeResult ReturnNotFound()
        {
            return StatusCode(StatusCodes.Status404NotFound);
        }
    }
}
