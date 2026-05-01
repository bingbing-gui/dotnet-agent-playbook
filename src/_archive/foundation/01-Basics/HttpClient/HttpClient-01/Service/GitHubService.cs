using AspNetCore.HttpClient.Models;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;

namespace AspNetCore.HttpClient.Service
{
    public class GitHubService
    {
        public System.Net.Http.HttpClient Client { get; }
        public GitHubService(System.Net.Http.HttpClient client)
        {
            client.BaseAddress = new Uri("https://api.github.com/");
            // GitHub API versioning
            client.DefaultRequestHeaders.Add("Accept",
                "application/vnd.github.v3+json");
            // GitHub requires a user-agent
            client.DefaultRequestHeaders.Add("User-Agent",
                "HttpClientFactory-Sample");
            Client = client;
        }
        public async Task<IEnumerable<GitHubIssue>> GetAspNetDocsIssues()
        {
            var response = await Client.GetAsync(
                "/repos/dotnet/AspNetCore.Docs/issues?state=open&sort=created&direction=desc");
            response.EnsureSuccessStatusCode();
            using var responseStream=await response.Content.ReadAsStreamAsync();
            return await JsonSerializer.DeserializeAsync
                <IEnumerable<GitHubIssue>>(responseStream);
        }
    }
}
