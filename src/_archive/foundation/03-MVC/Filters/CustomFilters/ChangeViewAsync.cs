using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace AspNetCore.Filters.CustomFilters
{
    public class ChangeViewAsync : Attribute, IAsyncResultFilter
    {
        public async Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
        {
            context.Result = new ViewResult()
            {
                ViewName = "List"
            };
            await next();
        }
    }
}
