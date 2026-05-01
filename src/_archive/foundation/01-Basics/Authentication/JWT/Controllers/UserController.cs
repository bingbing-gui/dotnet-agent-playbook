using AspNetCore.API.JWT.Authentication.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Security.Claims;
using System.Security.Cryptography;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using System.Buffers.Text;
using System.Collections;

namespace AspNetCore.API.JWT.Authentication.Controllers
{
    [ApiController]
    [Route("user")]
    public class UserController : ControllerBase
    {
        public readonly User _user;
        private readonly IConfiguration _configuration;
        public UserController(IConfiguration configuration, User user)
        {
            _configuration = configuration;
            _user = user;
        }
        /// <summary>
        /// 验证权限
        /// </summary>
        /// <returns></returns>
        [Authorize]
        [HttpGet]
        public ActionResult Get()
        {
            var username = HttpContext.User.Claims
               .FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?
               .Value;
            var response = new
            {
                Message = "授权成功",
                ServerTime = DateTime.Now,
                Username = username
            };
            return Ok(response);
        }
        /// <summary>
        /// 用户注册
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<ActionResult<User>> Post(UserDto request)
        {
            CreatePasswordHash(request.Password, out byte[] passwordHash, out byte[] passwordSalt);

            _user.Username = request.Username;
            _user.PasswordHash = passwordHash;
            _user.PasswordSalt = passwordSalt;
            Console.WriteLine(Convert.ToBase64String(passwordHash));
            Console.WriteLine(Convert.ToBase64String(passwordSalt));
            return Ok(_user);
        }
        private void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
        {
            using (var hmac = new HMACSHA512())
            {
                passwordSalt = hmac.Key;
                passwordHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
            }
        }
        /// <summary>
        /// 用户登录
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("login")]
        public async Task<ActionResult<string>> Login(UserDto request)
        {
            if (_user.Username != request.Username)
            {
                return BadRequest("User not found.");
            }
            if (!VerifyPasswordHash(request.Password, _user.PasswordHash, _user.PasswordSalt))
            {
                return BadRequest("Wrong password.");
            }
            string token = CreateToken(_user);

            var newRefreshToken = GenerateRefreshToken();
            SetRefreshToken(newRefreshToken);

            return Ok(token);
        }
        [HttpPost("refresh-token")]
        public async Task<ActionResult<string>> RefreshToken() 
        {
            var refreshToken = Request.Cookies["refreshToken"];

            if (!_user.RefreshToken.Equals(refreshToken))
            {
                return Unauthorized("Invalid Refresh Token.");
            }
            else if (_user.TokenExpires < DateTime.Now)
            {
                return Unauthorized("Token expired.");
            }
            string token = CreateToken(_user);
            var newRefreshToken = GenerateRefreshToken();
            SetRefreshToken(newRefreshToken);

            return Ok(token);
        }

        private bool VerifyPasswordHash(string password, byte[] passwordHash, byte[] passwordSalt)
        {
            using (var hmac = new HMACSHA512(passwordSalt))
            {
                var computedHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
                return computedHash.SequenceEqual(passwordHash);
            }
        }
        private string CreateToken(User user)
        {
            List<Claim> claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Username),
                new Claim(ClaimTypes.Role, "Admin")
            };
            var secret = _configuration.GetSection("JwtSetting:Secret").Value;
            var issue = _configuration.GetSection("JwtSetting:Issuer").Value;
            var audience = _configuration.GetSection("JwtSetting:Audience").Value;

            var key = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(secret));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

            var token = new JwtSecurityToken(
                issuer: issue,
                audience: audience,
                claims: claims,
                notBefore: DateTime.Now,
                expires: DateTime.Now.AddDays(1),
                signingCredentials: creds);

            var jwt = new JwtSecurityTokenHandler().WriteToken(token);

            return jwt;
        }


        private RefreshToken GenerateRefreshToken()
        {
            var refreshToken = new RefreshToken
            {
                Token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64)),
                Expires = DateTime.Now.AddDays(7),
                Created = DateTime.Now
            };
            return refreshToken;
        }
        private void SetRefreshToken(RefreshToken newRefreshToken)
        {
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Expires = newRefreshToken.Expires
            };
            Response.Cookies.Append("refreshToken", newRefreshToken.Token, cookieOptions);
            _user.RefreshToken = newRefreshToken.Token;
            _user.TokenCreated = newRefreshToken.Created;
            _user.TokenExpires = newRefreshToken.Expires;
        }

    }
}
