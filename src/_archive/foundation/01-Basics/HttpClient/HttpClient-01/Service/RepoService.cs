using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace AspNetCore.HttpClient.Service
{
    public class RepoService
    {
        private readonly System.Net.Http.HttpClient _httpClient;
        public RepoService(System.Net.Http.HttpClient httpClient)
        {
            _httpClient = httpClient;
        }
        public async Task<IEnumerable<string>> GetRepos()
        {
            var response = await _httpClient.GetAsync("aspnet/repos");
            response.EnsureSuccessStatusCode();
            using var responseStream = await response.Content.ReadAsStreamAsync();
            return await JsonSerializer.DeserializeAsync
                <IEnumerable<string>>(responseStream);
        }
    }
}
