using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using SolarflowClient.Models.ViewModels.Authentication;
using SolarflowClient.Models.ViewModels.Settings;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace SolarflowClient.Controllers
{
    public class SettingsController : Controller
    {
        private readonly HttpClient _httpClient;

        public SettingsController(HttpClient httpClient)
        {
            _httpClient = httpClient;
            _httpClient.BaseAddress = new Uri("https://localhost:7280/api/auth/");
            // _httpClient.BaseAddress = new Uri("https://solarflowapi.azurewebsites.net/api/auth/"); // CHANGE PRODUCTION.
        }

        public async Task<IActionResult> Index()
        {
            var token = Request.Cookies["AuthToken"];
            if (!string.IsNullOrEmpty(token) && token.StartsWith("Bearer "))
            {
                token = token.Substring("Bearer ".Length);
            }

            var requestMessage = new HttpRequestMessage(HttpMethod.Get, "get-user");
            requestMessage.Headers.Add("Authorization", $"Bearer {token}");

            var response = await _httpClient.SendAsync(requestMessage);
            if (response.IsSuccessStatusCode)
            {
                var userData = await response.Content.ReadAsStringAsync();
                var model = JsonConvert.DeserializeObject<GetUserViewModel>(userData);
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
        public async Task<IActionResult> UpdateUser(ChangeUserModelView model)
        {
            var token = Request.Cookies["AuthToken"];
            if (string.IsNullOrEmpty(token))
            {
                TempData["ErrorMessage"] = "You must be logged in to update your account.";
                return RedirectToAction("Login", "Authentication");
            }

            if (!ModelState.IsValid)
            {
                return View("Index", model);
            }

            if (!token.StartsWith("Bearer "))
            {
                token = "Bearer " + token;
            }

            var requestMessage = new HttpRequestMessage(HttpMethod.Post, "update-user"); // FIXED URL
            requestMessage.Headers.Add("Authorization", token);
            requestMessage.Content = new StringContent(JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json");

            var response = await _httpClient.SendAsync(requestMessage);
            if (response.IsSuccessStatusCode)
            {
                TempData["SuccessMessage"] = "User updated successfully!";
                return RedirectToAction("Index");
            }

            var errorResponse = await response.Content.ReadAsStringAsync();
            try
            {
                var errorObj = JsonConvert.DeserializeObject<dynamic>(errorResponse);
                TempData["ErrorMessage"] = errorObj?.error?.ToString() ?? "An unknown error occurred.";
            }
            catch
            {
                TempData["ErrorMessage"] = "An error occurred, but the response could not be parsed.";
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> DeleteUserViewAccount()
        {
            var token = Request.Cookies["AuthToken"];
            if (string.IsNullOrEmpty(token))
            {
                TempData["ErrorMessage"] = "You must be logged in to delete your account.";
                return RedirectToAction("Login", "Authentication");
            }
            if (!token.StartsWith("Bearer "))
            {
                token = "Bearer " + token;
            }
            var requestMessage = new HttpRequestMessage(HttpMethod.Post, "delete-user-view-model"); // FIXED URL
            requestMessage.Headers.Add("Authorization", token);
            var response = await _httpClient.SendAsync(requestMessage);
            if (response.IsSuccessStatusCode)
            {
                TempData["SuccessMessage"] = "User view account deleted successfully!";
                return RedirectToAction("Index");
            }
            var errorResponse = await response.Content.ReadAsStringAsync();
            try
            {
                var errorObj = JsonConvert.DeserializeObject<dynamic>(errorResponse);
                TempData["ErrorMessage"] = errorObj?.error?.ToString() ?? "An unknown error occurred.";
            }
            catch
            {
                TempData["ErrorMessage"] = "An error occurred, but the response could not be parsed.";
            }
            return RedirectToAction("Index");
        }

    }
}
