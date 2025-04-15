using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using SolarflowClient.Models;
using System.Net.Http;
using System.Threading.Tasks;

namespace SolarflowClient.Controllers
{
    public class MapController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        public MapController(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;

            if (_configuration["Environment"] == "Development")
            {
                _httpClient.BaseAddress = new Uri("https://localhost:7280/api/windy/");
            }
            else
            {
                _httpClient.BaseAddress = new Uri("https://solarflowapi.azurewebsites.net/api/windy/");
            }
        }

        public IActionResult Cloud()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetConfig()
        {
            var token = Request.Cookies["AuthToken"];
            if (string.IsNullOrEmpty(token))
                return Unauthorized();

            if (token.StartsWith("Bearer "))
                token = token.Substring("Bearer ".Length);

            var request = new HttpRequestMessage(HttpMethod.Get, "config");
            request.Headers.Add("Authorization", $"Bearer {token}");

            var response = await _httpClient.SendAsync(request);
            if (!response.IsSuccessStatusCode)
                return StatusCode((int)response.StatusCode);

            var json = await response.Content.ReadAsStringAsync();
            var config = JsonConvert.DeserializeObject<WindyConfig>(json);
            return Json(config);
        }
    }
}