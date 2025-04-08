using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using SolarflowClient.Models.ViewModels.Authentication;
using SolarflowClient.Models.ViewModels.Settings;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace SolarflowClient.Controllers
{
    /// <summary>
    /// Controller responsible for managing user settings, including retrieving, updating,
    /// creating, and deleting user and view accounts.
    /// </summary>
    public class SettingsController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        /// <summary>
        /// Initializes a new instance of the <see cref="SettingsController"/> class.
        /// Sets the API base address depending on the current environment (Development or Production).
        /// </summary>
        /// <param name="httpClient">The HTTP client used to communicate with the backend API.</param>
        /// <param name="configuration">Application configuration settings.</param>
        public SettingsController(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;

            if (_configuration["Environment"].Equals("Development")) 
            {
                _httpClient.BaseAddress = new Uri("https://localhost:7280/api/auth/");
            } else
            {
                _httpClient.BaseAddress = new Uri("https://solarflowapi.azurewebsites.net/api/auth/");
            }
        }

        /// <summary>
        /// Retrieves the current authenticated user's data for display on the settings page.
        /// Redirects to the login page if the user is not an admin or not authenticated.
        /// </summary>
        /// <returns>The settings view with user data or an error message.</returns>
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

        /// <summary>
        /// Updates the user's account information.
        /// Requires a valid authentication token and valid model input.
        /// </summary>
        /// <param name="model">The new user data to update.</param>
        /// <returns>
        /// Redirects to the Index view with a success message on success, or
        /// re-renders the form with validation errors or an error message on failure.
        /// </returns>
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

        /// <summary>
        /// Deletes the associated view-only account for the current user.
        /// </summary>
        /// <returns>
        /// Redirects to the Index view with a success or error message depending on the outcome.
        /// </returns>
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

        /// <summary>
        /// Creates a new view-only account for the authenticated user.
        /// </summary>
        /// <param name="Password">The password for the new view account.</param>
        /// <returns>
        /// Redirects to the Index view with a success or error message depending on the outcome.
        /// </returns>
        [HttpPost]
        public async Task<IActionResult> CreateViewAccount(string Password)
        {
            var token = Request.Cookies["AuthToken"];
            if (string.IsNullOrEmpty(token))
            {
                TempData["ErrorMessage"] = "You must be logged in to register a view account.";
                return RedirectToAction("Login", "Authentication");
            }

            if (!token.StartsWith("Bearer "))
            {
                token = "Bearer " + token;
            }

            var requestData = new { Password };
            var jsonContent = new StringContent(JsonConvert.SerializeObject(requestData), Encoding.UTF8, "application/json");

            var request = new HttpRequestMessage(HttpMethod.Post, "register-view")
            {
                Content = jsonContent
            };
            request.Headers.Add("Authorization", token);

            var response = await _httpClient.SendAsync(request);
            if (response.IsSuccessStatusCode)
            {
                TempData["SuccessMessage"] = "View account created successfully!";
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
