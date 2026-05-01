using Microsoft.AspNetCore.Antiforgery;

var builder = WebApplication.CreateBuilder();

builder.Services.AddAntiforgery(options =>
{
    options.Cookie.Name = "AntiForgery";
    options.Cookie.Domain = "localhost";
    options.Cookie.Path = "/";
    options.FormFieldName = "Antiforgery";
    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
});

var app = builder.Build();

//These are the four default services available at Configure
app.Run(async context =>
{
    var antiForgery = context.RequestServices.GetService<IAntiforgery>();
    if (HttpMethods.IsPost(context.Request.Method))
    {
        await antiForgery.ValidateRequestAsync(context);
        context.Response.Headers.Append("Content-Type", "text/html;charset=utf-8");
        await context.Response.WriteAsync("验证成功");
        return;
    }

    var token = antiForgery.GetAndStoreTokens(context);

    context.Response.Headers.Append("Content-Type", "text/html;charset=utf-8");
    await context.Response.WriteAsync($@"
    <html>
    <body>
        使用源代码查看生成的 anti forgery token
        <form method=""post"">
            <input type=""hidden"" name=""{token.FormFieldName}"" value=""{token.RequestToken}"" />
            <input type=""submit"" value=""Push""/>
        </form>
    </body>
    </html>   
    ");
});

app.Run();