using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;

namespace SolarflowServer.Services
{
    public class WindyService : IWindyService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;

        public WindyService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _apiKey = configuration["WindyAPI:ApiKey"];
        }

        public async Task<JObject> GetWeatherDataAsync(double latitude, double longitude)
        {
            string url = $"https://api.windy.com/api/point-forecast/v2?lat={latitude}&lon={longitude}&key={_apiKey}";

            var response = await _httpClient.GetAsync(url);
            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Erro ao chamar a Windy API: {response.StatusCode}");
            }

            var jsonResponse = await response.Content.ReadAsStringAsync();
            return JObject.Parse(jsonResponse);
        }
    }
}