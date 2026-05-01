using AspNetCore.Controllers.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting.Internal;

namespace AspNetCore.Controllers.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        private readonly IWebHostEnvironment _hostingEnvironment;
        public HomeController(ILogger<HomeController> logger, IWebHostEnvironment webHostEnvironment)
        {
            _hostingEnvironment = webHostEnvironment;
            _logger = logger;
        }
        public IActionResult Index()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Index(IFormFile photo)
        {
            using (var stream = new FileStream(Path.Combine(_hostingEnvironment.WebRootPath, photo.FileName), FileMode.Create))
            {
                await photo.CopyToAsync(stream);
            }
            return View();
        }
        public IActionResult SomeAction()
        {
            ViewData["Name"] = "GuiBingBing";
            ViewData["Address"] = new Address
            {
                HouseNo = "",
                City = ""
            };
            return View();
        }
        public IActionResult ReceivedDataByRequest()
        {
            var name = Request.Form["name"];
            var sex = Request.Form["sex"];
            return View("ReceivedDataByRequest", $"{name} sex is {sex}");
        }
        public IActionResult ReceivedDataByParameter(string name, string sex)
        {
            return View("ReceivedDataByParameter", $"{name} sex is {sex}");
        }
        public IActionResult ReceivedDataByModelBinding(Person person)
        {
            return View("ReceivedDataByModelBinding", person);
        }
    }
}