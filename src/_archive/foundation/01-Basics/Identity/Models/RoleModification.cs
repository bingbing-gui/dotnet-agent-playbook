using System.ComponentModel.DataAnnotations;

namespace Identity.Models
{
    public class RoleModification
    {
        [Required]
        public string RoleName { get; set; } = null!;

        public string RoleId { get; set; } = null!;

        public string[]? AddIds { get; set; }

        public string[]? DeleteIds { get; set; }
    }
}
