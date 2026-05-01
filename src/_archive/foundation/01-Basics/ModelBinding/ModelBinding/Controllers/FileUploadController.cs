using Microsoft.AspNetCore.Mvc;

namespace AspNetCore.ModelBinding.Controllers
{
    public class FileUploadController : Controller
    {
        private IWebHostEnvironment hostingEnvironment;
        public FileUploadController(IWebHostEnvironment environment)
        {
            hostingEnvironment = environment;
        }
        public IActionResult Index() => View();
        [HttpPost]
        public async Task<IActionResult> Index(IFormFile file)
        {
            string path = Path.Combine(hostingEnvironment.WebRootPath, "Images/" + file.FileName);
            using (var stream = new FileStream(path, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }
            return View((object)"Success");
        }
    }
}
