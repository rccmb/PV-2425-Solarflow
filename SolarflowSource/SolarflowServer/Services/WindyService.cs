using System.Text;
using System.Text.Json;
using SolarflowServer.Models;

namespace SolarflowServer.Services
{
    public class WindyService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        public WindyService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;
        }

        public async Task<List<FormattedForecast>> GetWeatherForecastAsync(double latitude, double longitude, int daysAhead)
        {
            var apiKey = _configuration["WindyAPI:Key"];
            var requestBody = new
            {
                lat = latitude,
                lon = longitude,
                model = "gfs",
                parameters = new[] {"temp", "hclouds", "lclouds", "mclouds" },
                levels = new[] { "surface" },
                key = apiKey
            };

            var jsonRequest = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("https://api.windy.com/api/point-forecast/v2", content);

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Erro Windy: {response.StatusCode} - {await response.Content.ReadAsStringAsync()}");
            }

            var responseContent = await response.Content.ReadAsStringAsync();
           var forecastData = JsonSerializer.Deserialize<WindyForecast>(responseContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            return forecastData?.GetFormattedForecast(daysAhead);
        }

        public async Task<SolarForecast> GetSolarForecastAsync(double latitude, double longitude, int dayAhead)
        {
            var forecasts = await GetWeatherForecastAsync(latitude, longitude, dayAhead);
            var filteredForecasts = forecasts.Where(f => DateTimeOffset.Parse(f.DateTime) <= DateTimeOffset.UtcNow.AddDays(3)).ToList();

            double solarExposure = SolarExposure(filteredForecasts, latitude);

            return new SolarForecast
            {
                Forecasts = filteredForecasts,
                SolarHours = solarExposure
            };
        }

        public double SolarExposure(List<FormattedForecast> forecasts, double latitude)
        {
            double totalSolarHours = 0;

            foreach (var forecast in forecasts)
            {
                DateTimeOffset forecastDate = DateTimeOffset.Parse(forecast.DateTime);
                double cloudCover = (forecast.HighClouds + forecast.MidClouds + forecast.LowClouds) / 3;
                double hour = forecastDate.UtcDateTime.Hour;
                bool isDaylight = hour >= 6 && hour <= 18;

                if (isDaylight)
                {
                    if (cloudCover < 20)
                        totalSolarHours += 3;
                    else if (cloudCover < 80)
                        totalSolarHours += 1.5;
                }
            }

            return totalSolarHours;
        }
    }


}
