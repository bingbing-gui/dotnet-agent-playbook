using Microsoft.AspNetCore.Mvc;

namespace AspNetCore.RouteLinks.Controllers
{
    public class ProductController : Controller
    {
        public string Index(int id)
        {
            return "Id Value is: " + id;
        }
        //public string List(int id)
        //{
        //    return "Id Value is: " + id;
        //}
        public IActionResult List()
        {
            return View();
        }
    }
}
