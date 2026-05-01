using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.Metrics;

namespace Identity.Models
{
    public class AppUser: IdentityUser<int>
    {
        public int Age
        {
            get; set;
        }
        public Country Country
        {
            get;
            set;
        }
        [Required]
        public string Salary
        {
            get; set;
        } = null!;
    }
       /// <summary>
   /// Represents a role in the system.
   /// </summary>
   public class Role : IdentityRole<int>
   {

        public Role()
        {
            
        }
        public Role(string name):base(name)
        {
            
        }
    }
    public enum Country
    {
        USA,
        UK,
        France,
        Germany,
        Russia
    }
}