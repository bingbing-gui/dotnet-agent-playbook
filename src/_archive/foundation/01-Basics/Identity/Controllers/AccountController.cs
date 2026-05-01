using Identity.CommonService;
using Identity.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using NETCore.MailKit.Core;
using System.ComponentModel.DataAnnotations;

namespace Identity.Controllers
{
    [Authorize]
    public class AccountController : Controller
    {
        private UserManager<AppUser> _userManager;
        private SignInManager<AppUser> _signInManager;
        private CommonService.IEmailService _emailService;
        public AccountController(UserManager<AppUser> userManager,
                                 SignInManager<AppUser> signInManager,
                                 CommonService.IEmailService emailService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _emailService = emailService;
        }

        [AllowAnonymous]
        public IActionResult Login(string returnUrl)
        {
            var login = new Login();
            login.ReturnUrl = returnUrl;
            return View(login);
        }
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(Login login)
        {
            if (ModelState.IsValid)
            {
                var appUser = await _userManager.FindByEmailAsync(login.Email);
                if (appUser != null)
                {
                    await _signInManager.SignOutAsync();
                    var signInResult = await _signInManager.PasswordSignInAsync(appUser, login.Password,
                        login.RememberMe, true);
                    if (signInResult.Succeeded)
                    {
                        return Redirect(login.ReturnUrl ?? "/");
                    }
                    var emailStatus = await _userManager.IsEmailConfirmedAsync(appUser);
                    if (emailStatus == false)
                    {
                        ModelState.AddModelError(nameof(login.Email), "Email为确认，请首先确认!");
                    }
                    if (signInResult.IsLockedOut)
                    {
                        ModelState.AddModelError(nameof(signInResult.IsLockedOut), "用户被锁定，请10分钟之后再次尝试");
                    }
                    #region 启用2FA登录
                    //if (appUser.TwoFactorEnabled)
                    //{
                    //    return RedirectToAction("LoginTwoStep", new { Email = appUser.Email, ReturnUrl = login.ReturnUrl });
                    //}
                    #endregion
                }
                ModelState.AddModelError(nameof(login.Email), "Login Failed: Invalid Email or password");
            }
            return View(login);
        }

        [AllowAnonymous]
        public async Task<IActionResult> LoginTwoStep(string email, string returnUrl)
        {
            var appUser = await _userManager.FindByEmailAsync(email);
            //创建Token
            var token = await _userManager.GenerateTwoFactorTokenAsync(appUser ?? new AppUser(), "Email");
            //发送邮件
            _emailService.Send(appUser?.Email ?? "bingbing.gui@outlook.com", "授权码", $"<h2>{token}</h2>");
            //发送SMS
            //_smsService.Send(appUser?.PhoneNumber ?? "13333333333", token);
            return View("LoginTwoStep", new TwoFactor { ReturnUrl = returnUrl });
        }
        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> LoginTwoStep(TwoFactor twoFactor, string returnUrl)
        {
            if (!ModelState.IsValid)
            {
                return View("LoginTwoStep", new TwoFactor { TwoFactorCode = twoFactor.TwoFactorCode, ReturnUrl = returnUrl });
            }
            var result = await _signInManager.TwoFactorSignInAsync("Email", twoFactor.TwoFactorCode, false, false);
            if (result.Succeeded)
            {
                return Redirect(returnUrl ?? "/");
            }
            else
            {
                ModelState.AddModelError("", "登录失败");
                return View();
            }
        }
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }
        public IActionResult AccessDenied()
        {
            return View();
        }
        [AllowAnonymous]
        public IActionResult ForgotPassword()
        {
            return View();
        }
        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> ForgotPassword([Required] string email)
        {
            if (!ModelState.IsValid)
            {
                return View(email);
            }
            var appUser = await _userManager.FindByEmailAsync(email);
            if (appUser == null)
            {
                return RedirectToAction(nameof(ForgotPasswordConfirmation));
            }
            var token = await _userManager.GeneratePasswordResetTokenAsync(appUser);
            var url = Url.Action("ResetPassword", "Account", new { Email = email, Token = token }, Request.Scheme);
            _emailService.Send(appUser!.Email ?? "450190369@qq.com", "重置密码", $"{url}");
            return RedirectToAction("ForgotPasswordConfirmation");
        }
        [AllowAnonymous]
        public IActionResult ForgotPasswordConfirmation()
        {
            return View();
        }
        [AllowAnonymous]
        public IActionResult ResetPassword(string token, string email)
        {
            var resetPassword = new ResetPassword() { Token = token, Email = email };
            return View(resetPassword);
        }
        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> ResetPassword(ResetPassword resetPassword)
        {
            if (!ModelState.IsValid)
                return View(resetPassword);
            var appUser = await _userManager.FindByEmailAsync(resetPassword.Email);
            if (appUser == null)
                RedirectToAction("ResetPasswordConfirmation");
            var resetPassResult = await _userManager.ResetPasswordAsync(appUser, resetPassword.Token, resetPassword.Password);
            if (!resetPassResult.Succeeded)
            {
                foreach (var error in resetPassResult.Errors)
                    ModelState.AddModelError(error.Code, error.Description);
                return View();
            }
            return RedirectToAction("ResetPasswordConfirmation");
        }
        [AllowAnonymous]
        public IActionResult ResetPasswordConfirmation()
        {
            return View();
        }
    }
}
