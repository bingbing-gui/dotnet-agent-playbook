namespace AspNetCore.Configuration.Middlewares
{
    public class ShortCircuitMiddleware
    {
        private RequestDelegate _next;
        public ShortCircuitMiddleware(RequestDelegate requestDelegate)
        {
            _next = requestDelegate;
        }
        public async Task Invoke(HttpContext context)
        {
            try
            {
                //if (context.Request.Headers["User-Agent"].Any(v => v.Contains("Firefox")))
                if (context.Items["Firefox"] as bool? == true)
                {
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                }
                else
                {
                    await _next(context);
                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }
}
