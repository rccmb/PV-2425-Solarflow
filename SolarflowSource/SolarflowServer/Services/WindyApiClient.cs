using System.Text;
using System.Text.Json;
using SolarflowServer.Models;

namespace SolarflowServer.Services
{
    // Responsible for communicating with the Windy API to fetch weather forecast data
    public class WindyApiClient
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;

        // Injects HttpClient and reads API key from configuration
        public WindyApiClient(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _apiKey = configuration["WindyAPI:Key"];
        }

        // Sends a POST request to Windy API to retrieve weather forecast based on coordinates
        public async Task<WindyForecast> GetWeatherForecastAsync(double latitude, double longitude)
        {
            // Prepare request body with required parameters
            var requestBody = new
            {
                lat = latitude,
                lon = longitude,
                model = "gfs", // Weather model used (Global Forecast System)
                parameters = new[] { "temp", "hclouds", "lclouds", "mclouds" }, // Required data layers
                levels = new[] { "surface" }, // Forecast level
                key = _apiKey // API key from config
            };

            var jsonRequest = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

            // Send the POST request to Windy API
            var response = await _httpClient.PostAsync("https://api.windy.com/api/point-forecast/v2", content);

            // If API call fails, throw detailed error for easier debugging
            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Windy API error: {response.StatusCode} - {await response.Content.ReadAsStringAsync()}");
            }

            // Parse response and return deserialized forecast data
            var responseContent = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<WindyForecast>(responseContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }
    }
}
