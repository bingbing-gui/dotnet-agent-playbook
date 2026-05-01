using Microsoft.Extensions.Primitives;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.Run(async context =>
{

    context.Response.Headers.Append("Content-Type", "text/html;charset=utf-8");

    StringValues queryString = context.Request.Query["message"];

    await context.Response.WriteAsync("<html><body>");
    await context.Response.WriteAsync("<h1>查询字符串中使用多个值</h1>");
    await context.Response.WriteAsync(@"<a href=""?message=hello&message=world&message=again"">点击查询添加查询字符串</a><br/><br/>");
    await context.Response.WriteAsync("<ul>");
    foreach (string v in queryString)
    {
        await context.Response.WriteAsync($"<li>{v}</li>");
    }
    await context.Response.WriteAsync("</ul>");
    await context.Response.WriteAsync("</body></html>");
   
});
app.Run();
