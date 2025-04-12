using System.Text;
using System.Text.Json;
using SolarflowServer.Models;

namespace SolarflowServer.Services
{
    /// <summary>
    /// Responsible for communicating with the Windy API to fetch weather forecast data.
    /// </summary>
    public class WindyApiClient
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;

        /// <summary>
        /// Initializes a new instance of the <see cref="WindyApiClient"/> class.
        /// Injects the HttpClient and reads the Windy API key from the configuration.
        /// </summary>
        /// <param name="httpClient">The HttpClient instance for making API requests.</param>
        /// <param name="configuration">The configuration object for reading the API key.</param>
        public WindyApiClient(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _apiKey = configuration["WindyAPI:Key"];
        }

        /// <summary>
        /// Sends a POST request to Windy API to retrieve a weather forecast based on the provided coordinates (latitude and longitude).
        /// </summary>
        /// <param name="latitude">The latitude of the location for which the weather forecast is requested.</param>
        /// <param name="longitude">The longitude of the location for which the weather forecast is requested.</param>
        /// <returns>A task that represents the asynchronous operation, containing the forecast data from the Windy API.</returns>
        /// <exception cref="Exception">Throws an exception if the API request fails.</exception>
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
