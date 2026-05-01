var app = WebApplication.Create();
app.Run(async context =>
{
    context.Response.Headers.Append("content-type", "text/html;;charset=utf-8");
    await context.Response.WriteAsync("<b>Hello world</b>");
    try
    {
        if (!context.Response.HasStarted)
        {
            context.Response.Headers.Append("X-USER", "Bill");
        }
    }
    catch (Exception ex)
    {
        await context.Response.WriteAsync($"<br/><br/>你不能修改Header集合，Body已经发送. Exception: {ex.Message}");
    }
});

app.Run();