using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace Identity.Models
{
    public class TwoFactor
    {

        [Required]
        [DisplayName("授权码")]
        public string TwoFactorCode { get; set; } = null!;
        public string? ReturnUrl { get; set; } 
    }
}
