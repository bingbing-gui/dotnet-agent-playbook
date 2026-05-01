using AspNetCore.Configuration.Services;
using Microsoft.AspNetCore.Diagnostics;

namespace AspNetCore.Configuration.Middlewares
{
    public class ContentMiddleware
    {
        private RequestDelegate _nextDelegate;
        private TotalUsers _totalUsers;
        public ContentMiddleware(RequestDelegate next, TotalUsers totalUsers)
        {
            _nextDelegate = next;
            _totalUsers = totalUsers;
        }
        public async Task Invoke(HttpContext httpContext)
        {
            try
            {
                if (httpContext.Request.Path == "/middleware")
                {
                    await httpContext.Response.WriteAsync("this message come from ContextMiddleware" + " TotalUsers=" + _totalUsers.TUsers());
                }
                else
                {
                    _nextDelegate(httpContext);
                    //var exceptionDetails = httpContext.Features.Get<IExceptionHandlerFeature>();
                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }
}
