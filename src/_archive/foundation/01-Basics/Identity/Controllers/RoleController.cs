using Identity.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace Identity.Controllers
{
    public class RoleController : Controller
    {
        private RoleManager<Role> _roleManager;
        private UserManager<AppUser> _userManager;
        public RoleController(RoleManager<Role> roleManager, UserManager<AppUser> userManager)
        {
            _roleManager = roleManager;
            _userManager = userManager;
        }
        #region 查询角色
        public IActionResult Index()
        {
            return View(_roleManager.Roles);
        }
        #endregion
        #region 创建角色
        public IActionResult Create() => View();

        [HttpPost]
        public async Task<IActionResult> CreateAsync([Required] string name)
        {
            if (ModelState.IsValid)
            {
                var result = await _roleManager.CreateAsync(new Role(name));
                if (result.Succeeded)
                    return RedirectToAction("Index");
                else
                    Errors(result);
            }
            return View(name);
        }
        #endregion
        #region 删除角色
        public async Task<IActionResult> DeleteAsync(string id)
        {
            var role = await _roleManager.FindByIdAsync(id);
            if (role != null)
            {
                var identityResult = await _roleManager.DeleteAsync(role);
                if (identityResult.Succeeded)
                {
                    return RedirectToAction("Index");
                }
                else
                {
                    Errors(identityResult);
                }
            }
            else
            {
                ModelState.AddModelError("", "No role found");
            }
            return View("Index", _roleManager.Roles);
        }
        #endregion
        #region 将用户添加到具体的角色中
        public async Task<IActionResult> UpdateAsync(string id)
        {
            var role = await _roleManager.FindByIdAsync(id);
            List<AppUser> members = new List<AppUser>();
            List<AppUser> nonMembers = new List<AppUser>();
            foreach (var appUser in _userManager.Users)
            {
                var list = await _userManager.IsInRoleAsync(appUser, role?.Name ?? "") ? members : nonMembers;
                list.Add(appUser);
            }
            return View(new RoleEdit() { Role = role, Members = members, NoMembers = nonMembers });
        }
        [HttpPost]
        public async Task<IActionResult> UpdateAsync(RoleModification roleModification)
        {
            if (ModelState.IsValid)
            {
                foreach (var userId in roleModification.AddIds ?? new string[] { })
                {
                    var appUser = await _userManager.FindByIdAsync(userId);
                    if (appUser != null)
                    {
                        var identityResult = await _userManager.AddToRoleAsync(appUser, roleModification.RoleName);
                        if (!identityResult.Succeeded)
                            Errors(identityResult);
                    }
                }
                foreach (var userId in roleModification.DeleteIds ?? new string[] { })
                {
                    var appUser = await _userManager.FindByIdAsync(userId);
                    if (appUser != null)
                    {
                        var identityResult = await _userManager.RemoveFromRoleAsync(appUser, roleModification.RoleName);
                        if (!identityResult.Succeeded)
                            Errors(identityResult);
                    }
                }
            }
            if (ModelState.IsValid)
                return RedirectToAction(nameof(Index));
            else
                return await UpdateAsync(roleModification.RoleId);
        }
        #endregion
        private void Errors(IdentityResult result)
        {
            foreach (IdentityError error in result.Errors)
                ModelState.AddModelError(error.Code, error.Description);
        }
    }
}
