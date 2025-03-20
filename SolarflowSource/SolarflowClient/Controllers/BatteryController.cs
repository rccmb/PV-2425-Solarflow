using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using SolarflowClient.Models.ViewModels.Battery;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace SolarflowClient.Controllers
{
    public class BatteryController : Controller
    {
        private readonly HttpClient _httpClient;

        public BatteryController(HttpClient httpClient)
        {
            _httpClient = httpClient;
            // _httpClient.BaseAddress = new Uri("https://localhost:7280/api/battery/");
            _httpClient.BaseAddress = new Uri("https://solarflowapi.azurewebsites.net/api/battery/");
        }

        public async Task<IActionResult> Index()
        {
            var token = Request.Cookies["AuthToken"];
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
                // Try to extract the error from JSON
                var errorResponse = await response.Content.ReadAsStringAsync();
                try
                {
                    var errorObj = JsonConvert.DeserializeObject<dynamic>(errorResponse);
                    TempData["ErrorMessage"] = errorObj.error != null ? errorObj.error.ToString() : errorResponse;
                }
                catch
                {
                    TempData["ErrorMessage"] = errorResponse;
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

            var errorMessage = await response.Content.ReadAsStringAsync();
            TempData["ErrorMessage"] = errorMessage;
            return RedirectToAction("Index");
        }
    }
}
