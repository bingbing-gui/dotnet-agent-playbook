var builder = WebApplication.CreateBuilder(args);
// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddAuthentication()
.AddCookie(CookieAuthenticationDefaults.AuthenticationScheme)
.AddGoogle("Google", options =>
{
    options.ClientId = builder.Configuration["Google:ClientId"] ?? throw new ArgumentNullException(nameof(options.ClientId), "GoogleClientId configuration is missing.");
    options.ClientSecret = builder.Configuration["Google:ClientSecret"] ?? throw new ArgumentNullException(nameof(options.ClientSecret), "GoogleClientSecret configuration is missing.");
    options.CallbackPath = $"/api/Auth/google/callback";
    options.SaveTokens = true; // Save tokens
    options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.Scope.Add("email");
    options.Scope.Add("profile");
    options.Events.OnRedirectToAuthorizationEndpoint = context =>
    {
        // 解码原始 redirect_uri
        var decoded = Uri.UnescapeDataString(context.RedirectUri);
        // 替换掉 http 为 https
        var corrected = decoded.Replace("http://", "https://");
        // 再重新编码
        var encoded = Uri.EscapeDataString(corrected);
        // 重新构造完整 URL（注意不重复编码 querystring）
        var redirectUrl = corrected; // 也可以直接用 corrected
        context.Response.Redirect(redirectUrl);
        return Task.CompletedTask;
    };
    options.Events.OnCreatingTicket = async context =>
    {
        var identity = context.Identity;
        var authService = context.HttpContext.RequestServices.GetRequiredService<IAuthService>();
        Console.WriteLine("claims: " + string.Join(", ", identity?.Claims.Select(c => $"{c.Type}:{c.Value}")));
        var email = identity?.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
        var name = identity?.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;
        Console.WriteLine("register email: " + email);
    };
})
.AddGitHub("GitHub", options =>
{
    options.ClientId = builder.Configuration["Github:ClientId"] ?? throw new ArgumentNullException(nameof(options.ClientId), "GitHubClientId configuration is missing.");
    options.ClientSecret = builder.Configuration["Github:ClientSecret"] ?? throw new ArgumentNullException(nameof(options.ClientSecret), "GitHubClientSecret configuration is missing.");
    options.CallbackPath = $"/api/Auth/github/callback";
    options.Scope.Add("user:email");
    options.SaveTokens = true; // Save tokens
    options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.Events.OnRedirectToAuthorizationEndpoint = context =>
    {
        // 解码原始 redirect_uri
        var decoded = Uri.UnescapeDataString(context.RedirectUri);
        // 替换掉 http 为 https
        var corrected = decoded.Replace("http://", "https://");
        // 再重新编码
        var encoded = Uri.EscapeDataString(corrected);
        // 重新构造完整 URL（注意不重复编码 querystring）
        var redirectUrl = corrected; // 也可以直接用 corrected
        context.Response.Redirect(redirectUrl);
        return Task.CompletedTask;
    };
    options.Events.OnCreatingTicket = async context =>
    {
        var accessToken = context.AccessToken;
        var refreshToken = context.RefreshToken;
        var identity = context.Identity;
        var authService = context.HttpContext.RequestServices.GetRequiredService<IAuthService>();
        Console.WriteLine("claims: " + string.Join(", ", identity?.Claims.Select(c => $"{c.Type}:{c.Value}")));
    };
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();
// 配置web api路由
app.MapControllers();
// 配置 MVC 路由
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();

