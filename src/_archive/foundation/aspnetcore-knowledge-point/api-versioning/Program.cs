
using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

// Add controllers
builder.Services.AddControllers();
builder.Services.AddControllersWithViews();
// Configure API versioning
builder.Services.AddApiVersioning(options =>
{
    options.ReportApiVersions = true; // Adds headers for supported and deprecated versions
});
builder.Logging.AddConsole();
builder.Services.AddControllersWithViews();
var app = builder.Build();

app.UseRouting(); // Enable routing

app.MapDefaultControllerRoute(); // Map attribute-routed controllers
app.Run();

[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiVersion("1.0")]
[ApiVersion("2.0")]
public class HelloWorldController : ControllerBase
{
    [HttpGet]
    public IActionResult Get(ApiVersion apiVersion)
        => Ok(new { Controller = GetType().Name, Version = apiVersion.ToString() });
}

public class HomeController : Controller
{
    public ActionResult Index()
    {
        return new ContentResult
        {
            Content = @"
                <html>
                    <body>
                    <ul>
                        <li><a href=""/api/v1/helloWorld"">Click here for Version 1</a></li>
                        <li><a href=""/api/v2/helloWorld"">Click here for Version 2</a></li>
                    </ul>
                    </body>
                </html>",
            ContentType = "text/html"
        };
    }
}