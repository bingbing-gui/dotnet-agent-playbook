using Microsoft.AspNetCore.Mvc.Filters;
using System.Diagnostics;
using System.Text;

namespace AspNetCore.Filters.CustomFilters
{
    public class TimeElapsedAsync : Attribute, IAsyncActionFilter
    {
        public async Task OnActionExecutionAsync(ActionExecutingContext context, 
                                           ActionExecutionDelegate next)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            await next();
            stopwatch.Stop();
            string result = "<div>Elapsed time: "
                + $"{stopwatch.Elapsed.TotalMilliseconds} ms</div>";
            byte[] bytes = Encoding.ASCII.GetBytes(result);
            await context.HttpContext.Response.Body.WriteAsync(bytes, 0, bytes.Length);

        }
    }
}
