using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;

namespace AspNetCore.GlobalizationLocalization.Models
{
    public class MyCultureProvider : RequestCultureProvider
    {
        public override Task<ProviderCultureResult> DetermineProviderCultureResult(HttpContext httpContext)
        {
            //var current_User = await user_Manager.GetUserAsync(HttpContext.User);
            //string user_culture = TblUserName.Where(c => c.Id == current_User.Id).Select(c => c.Culture).FirstOrDefault();
            //var requestCulture = new ProviderCultureResult(user_culture);​
            //return Task.FromResult(requestCulture);
            return null;
        }
    }
}
