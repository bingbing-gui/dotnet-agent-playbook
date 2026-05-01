using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;

namespace AspNetCore.Filters.CustomFilters
{
    public class HttpsOnly : Attribute, IAuthorizationFilter
    {
        public void OnAuthorization(AuthorizationFilterContext context)
        {
            if (!context.HttpContext.Request.IsHttps)
                context.Result = new StatusCodeResult(StatusCodes.Status403Forbidden);
        }
    }
}
