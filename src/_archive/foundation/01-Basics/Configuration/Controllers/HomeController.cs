using AspNetCore.Configuration.Services;
using Microsoft.AspNetCore.Mvc;
namespace AspNetCore.Configuration.Controllers
{
    public class HomeController : Controller
    {
        private TotalUsers _totalUsers;
        public HomeController(TotalUsers totalUsers)
        {
            _totalUsers = totalUsers;
        }
        public string Index()
        {
            return "总用户人数是" + _totalUsers.TUsers();
        }

        public IActionResult Exception() 
        {
            throw new NullReferenceException();
        }
        public IActionResult Error()
        {
            return View();
        }
    }
}
