using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using SolarflowClient.Models;
using SolarflowClient.Models.ViewModels.Authentication;
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
            var authToken = Request.Cookies["AuthToken"];
            var userEmail = Request.Cookies["UserEmail"];

            if (string.IsNullOrEmpty(authToken) || string.IsNullOrEmpty(userEmail))
            {
                return RedirectToAction("Login", "Account");
            }

            // Ensure the Authorization header is set correctly
            _httpClient.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authToken);

            try
            {
                var response = await _httpClient.GetAsync("get-user-battery");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var battery = JsonConvert.DeserializeObject<GetBatteryViewModel>(content);
                    ViewData["UserEmail"] = userEmail;
                    return View(battery);
                }

                // Log and display errors
                var errorMessage = await response.Content.ReadAsStringAsync();
                TempData["ErrorMessage"] = $"API Error: {response.StatusCode} - {response.ReasonPhrase}. Response: {errorMessage}";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Failed to retrieve battery: {ex.Message}";
            }

            return View(new GetBatteryViewModel());
        }



























        [HttpPost]
        public async Task<IActionResult> CreateBattery(CreateBatteryViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var content = new StringContent(JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync("create-battery", content);

                if (response.IsSuccessStatusCode)
                {
                    TempData["SuccessMessage"] = "Battery created successfully!";
                    return RedirectToAction("Index");
                }

                var errorMessage = await response.Content.ReadAsStringAsync();
                var errors = JsonConvert.DeserializeObject<List<ErrorDetail>>(errorMessage) ?? new List<ErrorDetail>();

                foreach (var error in errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"An error occurred: {ex.Message}");
            }

            return View(model);
        }

        public async Task<IActionResult> GetAllBatteries()
        {
            try
            {
                var response = await _httpClient.GetAsync("get-all-batteries");
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var batteries = JsonConvert.DeserializeObject<List<GetBatteryViewModel>>(content) ?? new List<GetBatteryViewModel>();
                    return View(batteries);
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Failed to retrieve batteries: {ex.Message}";
            }

            return View(new List<GetBatteryViewModel>());
        }

        public async Task<IActionResult> GetBatteryById(int id)
        {

            if (id <= 0)
            {
                return BadRequest("Invalid battery ID.");
            }

            try
            {
                var response = await _httpClient.GetAsync($"get-one-battery?id={id}");
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var battery = JsonConvert.DeserializeObject<GetBatteryViewModel>(content);
                    return View(battery);
                }
                else
                {
                    TempData["ErrorMessage"] = $"Error: {response.StatusCode} - {response.ReasonPhrase}";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Failed to retrieve battery: {ex.Message}";
            }

            return View();
        }

        public IActionResult Dashboard()
        {
            var userEmail = Request.Cookies["UserEmail"];
            ViewData["UserEmail"] = userEmail; // Pass to the view
            return View();
        }

        public async Task<IActionResult> GetUserBattery()
        {
            try
            {
                var response = await _httpClient.GetAsync("get-user-battery");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var battery = JsonConvert.DeserializeObject<GetBatteryViewModel>(content);

                    return View("UserBattery", battery); // Return data to a view
                }
                else
                {
                    TempData["ErrorMessage"] = $"Error: {response.StatusCode} - {response.ReasonPhrase}";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Failed to retrieve battery: {ex.Message}";
            }

            return View("UserBattery", null);
        }
    }
}
