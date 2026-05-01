using AspNetCore.XSS.Models;
using AspNetCore.XSS.Repository;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace AspNetCore.XSS.Controllers
{
    public class BlogController : Controller
    {
        private IBloggingRepository _repository;
        public BlogController(IBloggingRepository repository)
        {
            _repository = repository;
        }
        public async Task<IActionResult> Index()
        {
            var blogs=await _repository.GetAllBlogs();
            return View(blogs);
        }
        [HttpGet]
        public IActionResult AddBlog()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> AddBlog(Blog blog)
        {
            await _repository.AddBlog(blog);
            return View();
        }
        [HttpGet]
        public string GetProfile(string id)
        {
            return $"你好，我叫 {id}.";
        }
    }
}
