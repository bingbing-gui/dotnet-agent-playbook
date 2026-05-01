var app = WebApplication.Create();

app.Run(async context =>
{
    context.Response.Headers.Append("content-type", "text/html;charset=utf-8");
    var deleteCookie = context.Request.Query["delete"];

    if (!string.IsNullOrWhiteSpace(deleteCookie))
    {
        context.Response.Cookies.Delete("MyCookie");
        await context.Response.WriteAsync($@"<html><body>删除Cookie. 点击 <a href=""\"">这里</a> 返回到主页.</body></html>");
        return;
    }
    var cookie = context.Request.Cookies["MyCookie"];

    if (string.IsNullOrWhiteSpace(cookie) && context.Request.Path == "/")
    {
        context.Response.Cookies.Append
        (
            "MyCookie",
            "Hello World",
            new CookieOptions
            {
                Path = "/",
                Expires = DateTimeOffset.Now.AddDays(1)
            }
        );
        await context.Response.WriteAsync("<html><body>");
        await context.Response.WriteAsync($"写入一个新的Cookie<br/>刷新页面可以看到值.<br/>");
    }
    else
    {
        await context.Response.WriteAsync("<html><body>");
        await context.Response.WriteAsync($@"点击 <a href=""\?delete=1"">这里</a>删除 Cookie.<br/>");
    }
    await context.Response.WriteAsync($"Cookie的值为: {cookie}.");
    await context.Response.WriteAsync("</body></html>");
});

app.Run();
