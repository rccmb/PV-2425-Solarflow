using System.Text;
using System.Text.Json;
using SolarflowServer.Models;

namespace SolarflowServer.Services
{
    public class WindyApiClient
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;

        public WindyApiClient(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _apiKey = configuration["WindyAPI:Key"];
        }

        public async Task<WindyForecast> GetWeatherForecastAsync(double latitude, double longitude)
        {
            var requestBody = new
            {
                lat = latitude,
                lon = longitude,
                model = "gfs",
                parameters = new[] { "temp", "hclouds", "lclouds", "mclouds" },
                levels = new[] { "surface" },
                key = _apiKey
            };

            var jsonRequest = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("https://api.windy.com/api/point-forecast/v2", content);
            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Windy API error: {response.StatusCode} - {await response.Content.ReadAsStringAsync()}");
            }

            var responseContent = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<WindyForecast>(responseContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }
    }
}