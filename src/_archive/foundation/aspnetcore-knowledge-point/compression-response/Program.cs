using Microsoft.Net.Http.Headers;
using Microsoft.Extensions.Primitives;
using Microsoft.AspNetCore.ResponseCompression;

var builder = WebApplication.CreateBuilder();

//builder.Services.AddResponseCompression();
builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true; // 启用 HTTPS 的压缩
    //options.Providers.Add<GzipCompressionProvider>();
    //options.Providers.Add<CustomCompressionProvider>();
});

var app = builder.Build();

app.UseResponseCompression();

app.Run(async context =>
{
    var accept = context.Request.Headers[HeaderNames.AcceptEncoding];
    if (!StringValues.IsNullOrEmpty(accept))
    {
        context.Response.Headers.Append(HeaderNames.Vary, HeaderNames.AcceptEncoding);
    }
    context.Response.ContentType = "text/plain";
    var responseText = new string('A', 1000000); // 1,000,000 个字符
    await context.Response.WriteAsync(responseText);
});

app.Run();

