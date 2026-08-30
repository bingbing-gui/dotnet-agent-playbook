using _55_AspNetCoreMcpServer.Tools;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Net.Http.Headers;
using ModelContextProtocol.AspNetCore;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

Console.InputEncoding = Encoding.UTF8;
Console.OutputEncoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);

var builder = WebApplication.CreateBuilder(args);
var demoUsername = builder.Configuration["Auth:Username"] ?? throw new InvalidOperationException("Auth:Username must be configured.");
var demoPassword = builder.Configuration["Auth:Password"] ?? throw new InvalidOperationException("Auth:Password must be configured.");

var signingKeyValue = builder.Configuration["Auth:SigningKey"] ?? throw new InvalidOperationException("Auth:SigningKey must be configured.");
var signingKey = new SymmetricSecurityKey(Convert.FromBase64String(signingKeyValue));

// 仅当您确实需要允许浏览器跨源访问此服务器时，才启用 CORS
// 请将允许列表的范围严格限制在已知源。宽泛的 CORS 设置会降低安全性
builder.Services.AddCors(options =>
{
    options.AddPolicy("McpBrowserClient", policy =>
    {
        policy.WithOrigins("http://localhost:5164", "https://localhost:7133")
            .WithMethods("POST")
            .WithHeaders(HeaderNames.ContentType, HeaderNames.Authorization, "MCP-Protocol-Version")
            .WithExposedHeaders(HeaderNames.WWWAuthenticate);
    });
});

// 此服务器不发送“服务器到客户端”请求，因此使用无状态模式。
// 无状态模式支持无需会话亲和性的水平扩展，并兼容不发送 Mcp-Session-Id 的客户端。
builder.Services.AddMcpServer()
    .WithHttpTransport(options => options.SessionMode = HttpServerSessionMode.Stateless)
    .WithTools<WeatherTools>();


builder.Services.AddAuthentication(options =>
{
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidAudience = "AspNetCoreMcpServer",
        ValidIssuer = "everyone",
        IssuerSigningKey = signingKey,
        NameClaimType = ClaimTypes.Name,
        RoleClaimType = "roles"
    };
    options.Events = new JwtBearerEvents
    {
        OnTokenValidated = context =>
        {
            var name = context.Principal?.Identity?.Name ?? "unknown";
            var email = context.Principal?.FindFirstValue("preferred_username") ?? "unknown";
            Console.WriteLine($"Token validated for: {name} ({email})");
            return Task.CompletedTask;
        },
        OnAuthenticationFailed = context =>
        {
            Console.WriteLine($"Authentication failed: {context.Exception.Message}");
            return Task.CompletedTask;
        },
        OnChallenge = context =>
        {
            Console.WriteLine("Challenging client to provide a bearer token");
            return Task.CompletedTask;
        }
    };
});

builder.Services.AddAuthorization();

builder.Services.AddHttpContextAccessor();

// Open-Meteo 无需 API Key。定位请求限定 countryCode=CN，预报统一使用中国标准时间。
builder.Services.AddHttpClient("ChinaWeatherGeocoding", client =>
{
    client.BaseAddress = new Uri("https://geocoding-api.open-meteo.com/");
    client.Timeout = TimeSpan.FromSeconds(10);
});

builder.Services.AddHttpClient("ChinaWeatherForecast", client =>
{
    client.BaseAddress = new Uri("https://api.open-meteo.com/");
    client.Timeout = TimeSpan.FromSeconds(10);
});

var app = builder.Build();

app.UseCors();
app.UseAuthentication();
app.UseAuthorization();

app.MapMcp().RequireAuthorization().RequireCors("McpBrowserClient");

app.MapPost("/auth/token", (LoginRequest request) =>
{
    if (request.Username != demoUsername || request.Password != demoPassword)
    {
        return Results.Unauthorized();
    }

    var credentials = new SigningCredentials(
        signingKey,
        SecurityAlgorithms.HmacSha256);

    var token = new JwtSecurityToken(
        issuer: "everyone",
        audience: "AspNetCoreMcpServer",
        claims:
        [
            new Claim(ClaimTypes.Name, request.Username),
            new Claim("scope", "mcp:tools")
        ],
        expires: DateTime.UtcNow.AddHours(1),
        signingCredentials: credentials);

    return Results.Ok(new
    {
        access_token = new JwtSecurityTokenHandler().WriteToken(token),
        token_type = "Bearer",
        expires_in = 3600
    });
}).AllowAnonymous();


Console.WriteLine($"已启动带身份验证的 MCP Server：https://localhost:7049/");
Console.WriteLine($"Token URL: https://localhost:7049/auth/token");
Console.WriteLine($"演示用户名：{demoUsername}");
Console.WriteLine("按 Ctrl+C 停止服务器");

app.Run();

public sealed record LoginRequest(string Username, string Password);
