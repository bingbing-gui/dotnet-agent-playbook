using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace AspNetCore.ModelValidation.Infrastructure
{
    public class CustomDate : Attribute, IModelValidator
    {
        public IEnumerable<ModelValidationResult> Validate(ModelValidationContext context)
        {
            if (Convert.ToDateTime(context.Model) > DateTime.Now)
                return new List<ModelValidationResult> {
                    new ModelValidationResult("", "日期不能大于当前日期")
                };
            else if (Convert.ToDateTime(context.Model) < new DateTime(1980, 1, 1))
                return new List<ModelValidationResult> {
                    new ModelValidationResult("", "日期不能再1980年以前")
                };
            else
                return Enumerable.Empty<ModelValidationResult>();
        }
    }
}
