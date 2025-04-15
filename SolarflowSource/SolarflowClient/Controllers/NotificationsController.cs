using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using SolarflowClient.Models.ViewModels.Notifications;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;


namespace SolarflowClient.Controllers
{
    /// <summary>
    /// Controller responsible for handling notification-related operations such as listing,
    /// marking as read, and deleting notifications.
    /// </summary>
    public class NotificationsController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        /// <summary>
        /// Initializes a new instance of the <see cref="NotificationsController"/> class.
        /// Sets the API base address depending on the application's environment.
        /// </summary>
        /// <param name="httpClient">Injected HTTP client used for backend communication.</param>
        /// <param name="configuration">Application configuration service.</param>
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

        /// <summary>
        /// Fetches and displays a list of notifications for admin users.
        /// Redirects to login if the user is not an admin or if authorization fails.
        /// </summary>
        /// <returns>A view containing the list of notifications or an empty list with an error message.</returns>
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

        /// <summary>
        /// Marks a specific notification as read.
        /// </summary>
        /// <param name="id">The ID of the notification to mark as read.</param>
        /// <returns>Redirects to the Index view after marking the notification.</returns>
        [HttpPost]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            var token = Request.Cookies["AuthToken"];
            var request = new HttpRequestMessage(HttpMethod.Put, $"{id}/read");
            request.Headers.Add("Authorization", $"Bearer {token}");

            await _httpClient.SendAsync(request);
            return RedirectToAction("Index");
        }

        /// <summary>
        /// Deletes a specific notification.
        /// </summary>
        /// <param name="id">The ID of the notification to delete.</param>
        /// <returns>Redirects to the Index view after deletion.</returns>
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var token = Request.Cookies["AuthToken"];
            var request = new HttpRequestMessage(HttpMethod.Delete, $"{id}");
            request.Headers.Add("Authorization", $"Bearer {token}");

            await _httpClient.SendAsync(request);
            return RedirectToAction("Index");
        }

        /// <summary>
        /// Deletes all notifications for the current user.
        /// </summary>
        /// <returns>Redirects to the Index view after clearing all notifications.</returns>
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
