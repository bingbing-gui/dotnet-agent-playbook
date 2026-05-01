using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Diagnostics;

namespace AspNetCore.Filters.CustomFilters
{
    public class HybridActRes: ActionFilterAttribute
    {
        Stopwatch stopwatch;
        public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            stopwatch=Stopwatch.StartNew();
            await next();
        }
        public override async Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
        {
            stopwatch.Stop();
            context.Result = new ViewResult()
            {
                ViewName = "ShowTime",
                ViewData = new Microsoft.AspNetCore.Mvc.ViewFeatures.ViewDataDictionary(
                    new EmptyModelMetadataProvider(),
                    new ModelStateDictionary())
                {
                    Model = "Elapsed time: " + $"{stopwatch.Elapsed.TotalMilliseconds} ms"
                }
            };
            await next();
        }
    }
}
