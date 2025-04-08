using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using SolarflowClient.Models.ViewModels.Battery;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;

namespace SolarflowClient.Controllers
{
    /// <summary>
    /// Controller responsible for displaying and updating battery-related settings and data.
    /// Restricted to users with the Admin role.
    /// </summary>
    public class BatteryController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        /// <summary>
        /// Initializes a new instance of the <see cref="BatteryController"/> class.
        /// Sets the API base URL depending on the environment configuration.
        /// </summary>
        /// <param name="httpClient">HTTP client used to communicate with the API.</param>
        /// <param name="configuration">Application configuration provider.</param>
        public BatteryController(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;

            if (_configuration["Environment"].Equals("Development"))
            {
                _httpClient.BaseAddress = new Uri("https://localhost:7280/api/battery/");
            }
            else
            {
                _httpClient.BaseAddress = new Uri("https://solarflowapi.azurewebsites.net/api/battery/");
            }
        }

        /// <summary>
        /// Displays the battery settings view with current configuration data retrieved from the API.
        /// Only accessible to users with the Admin role.
        /// </summary>
        /// <returns>
        /// The battery settings view populated with current data, or redirects to login if unauthorized.
        /// </returns>
        public async Task<IActionResult> Index()
        {
            var token = Request.Cookies["AuthToken"];

            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(token);

            var role = jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;

            if (role.ToString() != "Admin")
            {
                return RedirectToAction("Login", "Authentication");
            }

            if (!string.IsNullOrEmpty(token) && token.StartsWith("Bearer "))
            {
                token = token.Substring("Bearer ".Length);
            }

            var requestMessage = new HttpRequestMessage(HttpMethod.Get, "get-battery");
            requestMessage.Headers.Add("Authorization", $"Bearer {token}");

            var response = await _httpClient.SendAsync(requestMessage);
            if (response.IsSuccessStatusCode)
            {
                var batteryData = await response.Content.ReadAsStringAsync();
                var model = JsonConvert.DeserializeObject<GetBatteryViewModel>(batteryData);
                return View(model);
            }
            else
            {
                var errorResponse = await response.Content.ReadAsStringAsync();
                try
                {
                    var errorObj = JsonConvert.DeserializeObject<dynamic>(errorResponse);
                    if (errorObj != null && errorObj.message != null)
                    {
                        TempData["ErrorMessage"] = errorObj.message.ToString();
                    }
                    else if (errorObj != null && errorObj.error != null)
                    {
                        TempData["ErrorMessage"] = errorObj.error.ToString();
                    }
                    else
                    {
                        TempData["ErrorMessage"] = "An unknown error occurred.";
                    }
                }
                catch
                {
                    TempData["ErrorMessage"] = "An error occurred, but the response could not be parsed.";
                }
                return View();
            }
        }

        /// <summary>
        /// Submits the updated battery configuration to the API.
        /// </summary>
        /// <param name="model">The updated battery settings data.</param>
        /// <returns>
        /// Redirects to the battery view with a success or error message based on the response.
        /// </returns>
        [HttpPost]
        public async Task<IActionResult> UpdateBattery(GetBatteryViewModel model)
        {
            var token = Request.Cookies["AuthToken"];
            if (string.IsNullOrEmpty(token))
            {
                TempData["ErrorMessage"] = "You must be logged in to update battery settings.";
                return RedirectToAction("Login", "Authentication");
            }

            if (!ModelState.IsValid)
            {
                return View("Index", model);
            }

            var requestMessage = new HttpRequestMessage(HttpMethod.Post, "update-battery");
            requestMessage.Headers.Add("Authorization", $"Bearer {token}");
            var content = new StringContent(JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json");
            requestMessage.Content = content;

            var response = await _httpClient.SendAsync(requestMessage);
            if (response.IsSuccessStatusCode)
            {
                TempData["SuccessMessage"] = "Battery settings updated successfully!";
                return RedirectToAction("Index");
            } 
            else
            {
                var errorResponse = await response.Content.ReadAsStringAsync();
                try
                {
                    var errorObj = JsonConvert.DeserializeObject<dynamic>(errorResponse);
                    if (errorObj != null && errorObj.message != null)
                    {
                        TempData["ErrorMessage"] = errorObj.message.ToString();
                    }
                    else if (errorObj != null && errorObj.error != null)
                    {
                        TempData["ErrorMessage"] = errorObj.error.ToString();
                    }
                    else
                    {
                        TempData["ErrorMessage"] = "An unknown error occurred.";
                    }
                }
                catch
                {
                    TempData["ErrorMessage"] = "An error occurred, but the response could not be parsed.";
                }
            }
            return RedirectToAction("Index");
        }
    }
}
