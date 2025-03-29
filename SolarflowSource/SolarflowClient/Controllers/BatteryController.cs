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
    //[Authorize(Roles = "Admin")]

    public class BatteryController : Controller
    {
        private readonly HttpClient _httpClient;

        public BatteryController(HttpClient httpClient)
        {
            _httpClient = httpClient;
            // _httpClient.BaseAddress = new Uri("https://localhost:7280/api/battery/");
            _httpClient.BaseAddress = new Uri("https://solarflowapi.azurewebsites.net/api/battery/"); // CHANGE PRODUCTION.
        }

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
