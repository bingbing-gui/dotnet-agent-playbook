using AspNetCore.UAParse.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using UAParser;

namespace AspNetCore.UAParse.Controllers;

public class HomeController : Controller
{
    public IActionResult Index()
    {
        var uaParser = Parser.GetDefault();

        // get a parser using externally supplied yaml definitions
        // var uaParser = Parser.FromYaml(yamlString);
        var userAgent = Request.Headers["User-Agent"].ToString();
        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
        
        ClientInfo c = uaParser.Parse(userAgent);

        Console.WriteLine(c.ToString());
        Console.WriteLine(c.UA.Family); // => "Mobile Safari"
        Console.WriteLine(c.UA.Major);  // => "5"
        Console.WriteLine(c.UA.Minor);  // => "1"
        Console.WriteLine(c.OS.Family);        // => "iOS"
        Console.WriteLine(c.OS.Major);         // => "5"
        Console.WriteLine(c.OS.Minor);         // => "1"
        Console.WriteLine(c.Device.Family);    // => "iPhone"


        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
