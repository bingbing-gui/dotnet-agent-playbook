using AspNetCore.ModelValidation.Models;
using Microsoft.AspNetCore.Mvc;
namespace AspNetCore.ModelValidation.Controllers
{
    public class JobController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
        [HttpPost]
        public IActionResult Index(JobApplication jobApplication)
        {
            #region
            //if (string.IsNullOrEmpty(jobApplication.Name))
            //    ModelState.AddModelError(nameof(jobApplication.Name), "请输入用户名");
            //else if (jobApplication.Name == "Osama Bin Laden")
            //    ModelState.AddModelError("", "你不能申请工作");
            //if (jobApplication.DOB == Convert.ToDateTime("01-01-0001 00:00:00"))
            //    ModelState.AddModelError(nameof(jobApplication.DOB), "请输入出生日期");
            //else if (jobApplication.DOB > DateTime.Now)
            //    ModelState.AddModelError(nameof(jobApplication.DOB), "出生日期不能大于当前时间");
            //else if (jobApplication.DOB < new DateTime(1980, 1, 1))
            //    ModelState.AddModelError(nameof(jobApplication.DOB), "出生日期不能在1980年以前");
            //if (string.IsNullOrEmpty(jobApplication.Sex))
            //    ModelState.AddModelError(nameof(jobApplication.Sex), "请选择性别");
            //if (jobApplication.Experience == "选择")
            //    ModelState.AddModelError(nameof(jobApplication.Experience), "请选择工作经验");
            //if (!jobApplication.TermsAccepted)
            //    ModelState.AddModelError(nameof(jobApplication.TermsAccepted), "必须接受条款");
            #endregion
            if (jobApplication.Name == "Osama Bin Laden")
                ModelState.AddModelError(nameof(jobApplication.Name), "You cannot apply for the Job");
            if (jobApplication.DOB > DateTime.Now)
                ModelState.AddModelError(nameof(jobApplication.DOB), "Date of Birth cannot be in the future");
            else if (jobApplication.DOB < new DateTime(1980, 1, 1))
                ModelState.AddModelError(nameof(jobApplication.DOB), "Date of Birth should not be before 1980");
            if (ModelState.IsValid)
                return View("Accepted", jobApplication);
            else
                return View();
        }

        public JsonResult ValidateDate(DateTime DOB)
        {
            if (DOB > DateTime.Now)
                return Json("日期必须大于当前时间");
            else if (DOB < new DateTime(1980, 1, 1))
                return Json("日期不能再1980年以前");
            else
                return Json(true);
        }
    }
}
