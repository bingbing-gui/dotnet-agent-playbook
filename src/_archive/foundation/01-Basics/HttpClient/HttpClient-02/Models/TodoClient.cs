using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace AspNetCore.HttpClientWithHttpVerb.Models
{
    public class TodoClient
    {
        private static readonly JsonSerializerOptions _jsonSerializerOptions = new JsonSerializerOptions
        {
            IgnoreNullValues = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
        private readonly HttpClient _httpClient;
        public TodoClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }
        public async Task<IEnumerable<TodoItem>> GetItemsAsync()
        {
            using var httpReponse = await _httpClient.GetAsync("/api/TodoItems");
            httpReponse.EnsureSuccessStatusCode();
            var stream = await httpReponse.Content.ReadAsStreamAsync();
            return await JsonSerializer.DeserializeAsync<List<TodoItem>>(stream, _jsonSerializerOptions);
        }
        public async Task<TodoItem> GetItemAsync(long id)
        {
            using var httpResponse = await _httpClient.GetAsync($"/api/TodoItems/{id}");
            if (httpResponse.StatusCode == HttpStatusCode.NotFound)
                return null;
            httpResponse.EnsureSuccessStatusCode();
            var stream = await httpResponse.Content.ReadAsStreamAsync();
            return await JsonSerializer.DeserializeAsync<TodoItem>(stream, _jsonSerializerOptions);
        }
        public async Task CreateItemAsync(TodoItem todoItem)
        {
            var todoItemJson = new StringContent(
                JsonSerializer.Serialize(todoItem, _jsonSerializerOptions),
                System.Text.Encoding.UTF8,
                "application/json");
            using var httpResponse = await _httpClient.PostAsync("/api/TodoItems", todoItemJson);

            httpResponse.EnsureSuccessStatusCode();
        }
        public async Task SaveItemAsync(TodoItem todoItem)
        {
            var todoItemJson = new StringContent(
                JsonSerializer.Serialize(todoItem, _jsonSerializerOptions),
                System.Text.Encoding.UTF8,
                 "application/json"
            );
            using var httpResponse = await _httpClient.PutAsync($"/api/TodoItems/{todoItem.Id}", todoItemJson);
            httpResponse.EnsureSuccessStatusCode();
        }
        #region Delete Request
        public async Task DeleteItemAsync(long id)
        {
            using var httpResponse = await _httpClient.DeleteAsync($"/api/TodoItems/{id}");
            httpResponse.EnsureSuccessStatusCode();
        }
        #endregion
    }

}
