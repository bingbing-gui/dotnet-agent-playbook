using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace Identity.Models
{
    public class ResetPassword
    {
        [Required]
        [DisplayName("密码")]
        public string Password { get; set; } = null!;

        [Compare("Password", ErrorMessage = "密码和确认密码不匹配")]
        [DisplayName("确认密码")]
        public string ConfirmPassword { get; set; } = null!;

        public string Email { get; set; } = null!;
        public string Token { get; set; } = null!;
    }
}
