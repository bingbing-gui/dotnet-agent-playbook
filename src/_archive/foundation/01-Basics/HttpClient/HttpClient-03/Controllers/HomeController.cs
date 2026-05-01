using AspNetCore.HttpClientHander.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using System.Diagnostics;
using System.Text.Json;

namespace AspNetCore.HttpClientHander.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IHttpClientFactory _clientFactory;
        public HomeController(ILogger<HomeController> logger, IHttpClientFactory clientFactory)
        {
            _logger = logger;
            _clientFactory = clientFactory;
        }
        public IActionResult Index()
        {

            return View();
        }
        public async Task<IActionResult> HttpMessageHandler()
        {
            var request = new HttpRequestMessage(HttpMethod.Get, "https://api.github.com/repos/dotnet/AspNetCore.Docs/pulls")
            {
                Headers =
                {
                    { HeaderNames.Accept, "application/vnd.github.v3+json" },
                    { HeaderNames.UserAgent, "HttpRequestsSample" },
                    { "X-API-KEY", Guid.NewGuid().ToString()}
                }
            };
            var httpClient = _clientFactory.CreateClient("HttpMessageHandler");
            var response = await httpClient.SendAsync(request);
            NamedClientModel namedClientModel = new NamedClientModel();
            if (response.IsSuccessStatusCode)
            {
                using var responseStream = await response.Content.ReadAsStreamAsync();
                var PullRequests = await JsonSerializer.DeserializeAsync
                         <IEnumerable<GitHubPullRequest>>(responseStream);
                namedClientModel.GetPullRequestsError = false;
                namedClientModel.PullRequests = PullRequests;
            }
            else
            {
                namedClientModel.GetPullRequestsError = true;
                namedClientModel.PullRequests = Array.Empty<GitHubPullRequest>();
            }
            return View(namedClientModel);
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
}