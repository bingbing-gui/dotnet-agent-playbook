using AspNetCore.GlobalLocalResFiles.Infrastructure;
using System.ComponentModel.DataAnnotations;
using System.Xml.Linq;

namespace AspNetCore.GlobalLocalResFiles.Models
{
    public class JobApplication
    {
        [Required(ErrorMessage = "Please provide your name")]
        [Display(Name = "Job applicant name")]
        public string Name { get; set; }

        [RegularExpression("^[a-zA-Z0-9_\\.-]+@([a-zA-Z0-9-]+\\.)+[a-zA-Z]{2,6}$", ErrorMessage = "E-mail is not valid")]
        [Display(Name = "Job applicant email")]
        public string Email { get; set; }
        [CustomDate]
        [Display(Name = "Date of Birth")]
        public DateTime DOB { get; set; }
        [Required(ErrorMessage = "Please select your sex")]
        [Display(Name = "Job applicant sex")]
        public string Sex { get; set; }
        [Range(2, 4, ErrorMessage = "{0} must be a number between {1} and {2}")]
        [Display(Name = "Job applicant experience")]
        public int Experience { get; set; }
        [Range(typeof(bool), "true", "true", ErrorMessage = "You must accept the Terms")]
        [Display(Name = "Terms")]
        public bool TermsAccepted { get; set; }
    }
}
