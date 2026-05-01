using AspNetCore.ModelValidation.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace AspNetCore.ModelValidation.Models
{
    //public class JobApplication
    //{
    //    [DisplayName("姓名")]
    //    public string? Name { get; set; }
    //    [DisplayName("出生日期")]
    //    public DateTime DOB { get; set; }
    //    [DisplayName("性别")]
    //    public string? Sex { get; set; }
    //    [DisplayName("工作经验")]
    //    public string? Experience { get; set; }
    //    [DisplayName("条款")]
    //    public bool TermsAccepted { get; set; }
    //}
    public class JobApplication
    {
        [Required(ErrorMessage ="姓名不能为空")]
        [DisplayName("姓名")]
        [NameValidate(NotAllowed = new string[] { "Osama Bin Laden", "Saddam Hussain", "Mohammed Gaddafi" }, 
            ErrorMessage = "你不能申请这份工作")]
        public string Name { get; set; }
        
        [DisplayName("出生日期")]
        [Required(ErrorMessage = "请输入你的出生日期")]
        [Remote("ValidateDate", "Job")]
        public DateTime? DOB { get; set; }

        [Required(ErrorMessage = "请选择性别")]
        [DisplayName("性别")]
        public string Sex { get; set; }

        [Range(0, 5,ErrorMessage ="工作年限必须在0-5年")]
        [DisplayName("工作经验")]
        public string? Experience { get; set; }

        [Range(typeof(bool), "true", "true", ErrorMessage = "你必须接受条款")]
        [DisplayName("条款")]
        public bool TermsAccepted { get; set; }

        [RegularExpression("^[a-zA-Z0-9_\\.-]+@([a-zA-Z0-9-]+\\.)+[a-zA-Z]{2,6}$", ErrorMessage = "电子邮件不正确")]
        [DisplayName("电子邮件")]
        [Required(ErrorMessage="电子邮件不能为空")]
        public string Email { get; set; }
    }
}
