using Identity.CommonService;
using Identity.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Identity.Controllers
{
    public class AdminController : Controller
    {
        private UserManager<AppUser> _userManager;
        private IPasswordHasher<AppUser> _passwordHasher;
        private IPasswordValidator<AppUser> _passwordValidator;
        private IUserValidator<AppUser> _userValidator;
        private IEmailService _emailService;
        public AdminController(UserManager<AppUser> userManager,
                               IPasswordHasher<AppUser> passwordHash,
                               IPasswordValidator<AppUser> passwordValidator,
                               IUserValidator<AppUser> userValidator,
                               IEmailService emailService
                               )
        {
            _userManager = userManager;
            _passwordHasher = passwordHash;
            _passwordValidator = passwordValidator;
            _userValidator = userValidator;
            _emailService = emailService;
        }
        public IActionResult Index()
        {
            return View(_userManager.Users);
        }
        public ViewResult Create() => View();
        [HttpPost]
        public async Task<IActionResult> Create(User user)
        {
            if (ModelState.IsValid)
            {
                AppUser appUser = new AppUser
                {
                    UserName = user.Name,
                    Email = user.Email,
                    Age = user.Age,
                    //TwoFactorEnabled = true,
                    Country = user.Country,
                    Salary = user.Salary
                };
                var identityResult = await _userManager.CreateAsync(appUser, user.Password);
                if (identityResult.Succeeded)
                {
                    #region 生成电子邮件确认
                    // var token = await _userManager.GenerateEmailConfirmationTokenAsync(appUser);
                    // var confirmationLink = Url.Action("ConfirmEmail", "Email", new { token, Email = appUser.Email }, Request.Scheme);
                    // _emailService.Send(user.Email, "电子邮件确认", $"<h2>{confirmationLink}</h2>");
                    #endregion
                    return RedirectToAction("Index", "Admin");
                }
                else
                    foreach (IdentityError error in identityResult.Errors)
                        ModelState.AddModelError("", error.Description);
            }
            return View(user);
        }
        public async Task<IActionResult> Update(string Id)
        {
            var appUser = await _userManager.FindByIdAsync(Id);
            if (appUser != null)
            {
                var updateUserDTO = new UpdateUserDTO();
                updateUserDTO.Id = appUser.Id;
                updateUserDTO.Name = appUser.UserName ?? "";
                updateUserDTO.Email = appUser.Email == null ? "" : appUser.Email;
                updateUserDTO.Age = appUser.Age;
                updateUserDTO.Country = appUser.Country;
                updateUserDTO.Salary = appUser.Salary;
                return View(updateUserDTO);
            }
            else
            {
                return RedirectToAction("Index");
            }
        }
        [HttpPost]
        public async Task<IActionResult> Update(UpdateUserDTO updateUserDTO)
        {
            var appUser = await _userManager.FindByIdAsync(updateUserDTO.Id.ToString());
            if (appUser != null)
            {
                IdentityResult validEmail = null;
                if (!string.IsNullOrEmpty(updateUserDTO.Name) && !string.IsNullOrEmpty(updateUserDTO.Email))
                {
                    appUser.UserName = updateUserDTO.Name;
                    appUser.Email = updateUserDTO.Email;
                    validEmail = await _userValidator.ValidateAsync(_userManager, appUser);
                    if (!validEmail.Succeeded)
                        Errors(validEmail);
                }
                else
                    ModelState.AddModelError("", "用户名和邮件不能为空");

                appUser.Age = updateUserDTO.Age;
                appUser.Country = updateUserDTO.Country;
                if (!string.IsNullOrEmpty(updateUserDTO.Salary))
                    appUser.Salary = updateUserDTO.Salary;

                IdentityResult validPass = null;
                if (!string.IsNullOrEmpty(updateUserDTO.Password))
                {
                    validPass = await _passwordValidator.ValidateAsync(_userManager, appUser, updateUserDTO.Password);
                    if (validPass.Succeeded)
                        appUser.PasswordHash = _passwordHasher.HashPassword(appUser, updateUserDTO.Password);
                    else
                        Errors(validPass);
                }
                else
                    ModelState.AddModelError("", "密码不能为空");
                if (!string.IsNullOrEmpty(updateUserDTO.Name) &&
                    !string.IsNullOrEmpty(updateUserDTO.Email) &&
                    !string.IsNullOrEmpty(updateUserDTO.Password) &&
                    validEmail.Succeeded &&
                    validPass.Succeeded)
                {
                    var result = await _userManager.UpdateAsync(appUser);
                    if (result.Succeeded)
                        return RedirectToAction("Index");
                    else
                    {
                        foreach (IdentityError error in result.Errors)
                        {
                            ModelState.AddModelError("", error.Description);
                        }
                    }
                }
            }
            else
                ModelState.AddModelError("", "没有发现该用户");
            return View(updateUserDTO);
        }
        [HttpPost]
        public async Task<IActionResult> Delete(string Id)
        {
            var appUser = await _userManager.FindByIdAsync(Id);
            if (appUser != null)
            {
                var result = await _userManager.DeleteAsync(appUser);
                if (result.Succeeded)
                    return RedirectToAction("Index");
                else
                    Errors(result);
            }
            else
                ModelState.AddModelError("", "没有发现该用户");
            return View("Index", _userManager.Users);
        }
        void Errors(IdentityResult result)
        {
            foreach (IdentityError error in result.Errors)
                ModelState.AddModelError("", error.Description);
        }
    }
}
