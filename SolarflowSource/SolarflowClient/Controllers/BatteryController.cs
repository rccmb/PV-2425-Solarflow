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
            _httpClient.BaseAddress = new Uri("https://localhost:7280/api/battery/");
        }

        public async Task<IActionResult> Index()
        {
            var token = Request.Cookies["AuthToken"];
            if (string.IsNullOrEmpty(token))
            {
                TempData["ErrorMessage"] = "You must be logged in to view battery information.";
                return RedirectToAction("Login", "Authentication");
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

            TempData["ErrorMessage"] = "Error fetching battery data.";
            return RedirectToAction("Index", "Home");
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
