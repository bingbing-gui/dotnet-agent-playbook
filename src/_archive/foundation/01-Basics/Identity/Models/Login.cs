using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Identity.Models
{
    public class Login
    {
        [Required]
        [DisplayName("用户名")]
        public string Email { get; set; } = null!;
        [Required]
        [DisplayName("密码")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = null!;
        public string? ReturnUrl { get; set; }

        [Required]
        [DisplayName("记住密码")]
        public bool RememberMe { get; set; }
    }
}
