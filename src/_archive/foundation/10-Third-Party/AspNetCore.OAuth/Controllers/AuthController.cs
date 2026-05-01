using AspNet.Security.OAuth.GitHub;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Mvc;

[Route("api/[controller]")]
[ApiController]
public class AuthController : ControllerBase
{
    /// <summary>
    /// SIGN IN WITH GOOGLE
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    [HttpGet("google/login")]
    public async Task<IActionResult> SignInWithGoogle()
    {
        return Challenge(GoogleDefaults.AuthenticationScheme);
    }

    [HttpGet("github/login")]
    public IActionResult SigInWithGithub()
    {
        return Challenge(GitHubAuthenticationDefaults.AuthenticationScheme);
    }
}