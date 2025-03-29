using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using SolarflowClient.Models.ViewModels.Notifications;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;

namespace SolarflowClient.Controllers
{
    public class NotificationsController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        public NotificationsController(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;

            if (_configuration["Environment"].Equals("Development"))
            {
                _httpClient.BaseAddress = new Uri("https://localhost:7280/api/notifications/");
            } else
            {
                _httpClient.BaseAddress = new Uri("https://solarflowapi.azurewebsites.net/api/notifications/");
            }
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
                token = token.Substring("Bearer ".Length);

            var requestMessage = new HttpRequestMessage(HttpMethod.Get, "");
            requestMessage.Headers.Add("Authorization", $"Bearer {token}");

            var response = await _httpClient.SendAsync(requestMessage);
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();

                var notifications = JsonConvert.DeserializeObject<List<GetNotificationsViewModel>>(json);
                return View(notifications);
            }

            TempData["ErrorMessage"] = "Unable to load notifications.";
            return View(new List<GetNotificationsViewModel>());
        }

        [HttpPost]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            var token = Request.Cookies["AuthToken"];
            var request = new HttpRequestMessage(HttpMethod.Put, $"{id}/read");
            request.Headers.Add("Authorization", $"Bearer {token}");

            await _httpClient.SendAsync(request);
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var token = Request.Cookies["AuthToken"];
            var request = new HttpRequestMessage(HttpMethod.Delete, $"{id}");
            request.Headers.Add("Authorization", $"Bearer {token}");

            await _httpClient.SendAsync(request);
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> DeleteAll()
        {
            var token = Request.Cookies["AuthToken"];
            var request = new HttpRequestMessage(HttpMethod.Delete, "");
            request.Headers.Add("Authorization", $"Bearer {token}");

            await _httpClient.SendAsync(request);
            return RedirectToAction("Index");
        }
    }
}
