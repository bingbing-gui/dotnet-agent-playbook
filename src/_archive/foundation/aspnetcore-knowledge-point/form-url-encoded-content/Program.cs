var app = WebApplication.Create();
app.Run(async context =>
{
    var dicts = new Dictionary<string, string>()
    {
        ["id"] = "001",
        ["name"] = "桂兵兵",
        ["birthday"] = "1986/08/30",
        ["guid"] = Guid.NewGuid().ToString(),
        ["artist"] = "Bill Gui",
        ["formula"] = "10 * 5 = 50"
    };
    using var queryString = new FormUrlEncodedContent(dicts);
    context.Response.Headers.Append("Content-Type", "text/html;charset=utf-8");
    await context.Response.WriteAsync($@"<html>
    <head>
        <link rel=""stylesheet"" href=""https://cdnjs.cloudflare.com/ajax/libs/bulma/0.7.5/css/bulma.css"" />
    </head>
    <body class=""content"">
    <div class=""container"">
    <h1>使用 FormUrlEncodedContent 创建编码的URL字符串</h1>
    <strong>输入</strong>
    ");
    await context.Response.WriteAsync("<ul>");
    foreach (var k in dicts)
    {
        await context.Response.WriteAsync($"<li>{k.Key} = {k.Value}</li>");
    }
    await context.Response.WriteAsync("</ul>");
    await context.Response.WriteAsync("<strong>输出</strong><br/>");
    await context.Response.WriteAsync(await queryString.ReadAsStringAsync());
    await context.Response.WriteAsync("</ul>");
    await context.Response.WriteAsync(@"</div></body></html>");
});
app.Run();