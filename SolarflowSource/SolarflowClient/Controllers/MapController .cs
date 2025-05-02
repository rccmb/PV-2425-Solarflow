using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using SolarflowClient.Models;
using System.Net.Http;
using System.Threading.Tasks;

namespace SolarflowClient.Controllers
{
    /// <summary>
    /// Controller responsible for handling map-related views and API interactions with the Windy service.
    /// </summary>
    public class MapController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        /// <summary>
        /// Initializes a new instance of the <see cref="MapController"/> class.
        /// Configures the HTTP client base address based on the application environment.
        /// </summary>
        /// <param name="httpClient">The HTTP client used for API communication.</param>
        /// <param name="configuration">The application configuration service.</param>
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

        /// <summary>
        /// Displays the cloud map view.
        /// </summary>
        /// <returns>The cloud map view.</returns>
        public IActionResult Cloud()
        {
            return View();
        }

        /// <summary>
        /// Retrieves the Windy configuration from the API.
        /// </summary>
        /// <returns>A JSON object containing the Windy configuration, or an appropriate HTTP status code if the request fails.</returns>
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