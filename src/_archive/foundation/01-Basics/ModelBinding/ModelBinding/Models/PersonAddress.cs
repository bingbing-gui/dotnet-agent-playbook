using Microsoft.AspNetCore.Mvc.ModelBinding;
namespace AspNetCore.ModelBinding.Models
{
    public class PersonAddress
    {
        public string City { get; set; }
        [BindNever]
        public string Country { get; set; }
    }
}
