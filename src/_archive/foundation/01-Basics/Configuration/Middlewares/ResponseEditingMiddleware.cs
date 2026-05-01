namespace AspNetCore.Configuration.Middlewares
{
    public class ResponseEditingMiddleware
    {
        private RequestDelegate _next;
        public ResponseEditingMiddleware(RequestDelegate next)
        {
            _next = next;
        }
        public async Task Invoke(HttpContext httpContext)
        {
            try
            {
                await _next(httpContext);

                if (httpContext.Response.StatusCode == 401)
                {
                    await httpContext.Response.WriteAsync("Firefox browser not authorized");
                }
                else if (httpContext.Response.StatusCode == 404)
                {
                    await httpContext.Response.WriteAsync("No Response Generated");
                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }
}
