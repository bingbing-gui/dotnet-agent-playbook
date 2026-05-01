using Microsoft.Extensions.Localization;
using System.ComponentModel.DataAnnotations;

namespace AspNetCore.GlobalLocalResFiles.Infrastructure
{
    public class CustomDate : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var _localizationService = (IStringLocalizer<CustomDate>)validationContext.GetService(typeof(IStringLocalizer<CustomDate>));
            if ((DateTime)value > DateTime.Now)
                return new ValidationResult(_localizationService["Date of Birth cannot be in the future"]);
            else if ((DateTime)value < new DateTime(1980, 1, 1))
                return new ValidationResult(_localizationService["Date of Birth should not be before 1980"]);
            return ValidationResult.Success;
        }
    }
}
