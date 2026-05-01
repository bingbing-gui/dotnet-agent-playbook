using System.Text.Json;

namespace AspNetCore.Views.Service
{
    public class Joke
    {
        public async Task<string> GetJoke()
        {
            string apiResponse = "";
            using (var httpClient = new HttpClient())
            {
                using (var response = await httpClient.GetAsync("https://api.vvhan.com/api/joke"))
                {
                    apiResponse = await response.Content.ReadAsStringAsync();
                }
            }
            return apiResponse;
        }
    }
}
