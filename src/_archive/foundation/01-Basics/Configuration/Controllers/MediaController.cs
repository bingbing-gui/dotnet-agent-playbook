using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting.Internal;

namespace AspNetCore.Configuration.Controllers
{
    public class MediaController: Controller
    {
        private IWebHostEnvironment _env;
        public MediaController(IWebHostEnvironment hostingEnvironment)
        {

            _env = hostingEnvironment;

        }
        public IActionResult Index()
        {
            string absolutePath = Path.Combine(_env.WebRootPath, "file1.jpg");
            ViewBag.ImagePath = absolutePath;
            return View();
        }
    }
}
