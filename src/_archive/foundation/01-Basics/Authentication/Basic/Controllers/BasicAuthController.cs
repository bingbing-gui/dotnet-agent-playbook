using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AspNetCore.API.BasicAuthentication.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class BasicAuthController : ControllerBase
    {
        public BasicAuthController()
        {

        }
        [Authorize]
        [HttpGet("Auth", Name = "BasicAuth")]
        public ActionResult BasicAuth()
        {
            var username = HttpContext.User.Claims
               .FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?
               .Value;
            var response = new
            {
                Message = "授权成功",
                ServerTime = DateTime.Now,
                Username= username
            };
            return Ok(response);
        }
    }
}