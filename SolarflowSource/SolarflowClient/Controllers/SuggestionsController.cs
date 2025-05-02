using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using SolarflowClient.Models.ViewModels.Suggestions;

namespace SolarflowClient.Controllers
{
    /// <summary>
    /// Controller responsible for generating, retrieving, applying, and ignoring optimization suggestions
    /// for a user based on their data.
    /// </summary>
    public class SuggestionsController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        /// <summary>
        /// Initializes a new instance of the <see cref="SuggestionsController"/> class.
        /// Configures the API base address based on the current environment.
        /// </summary>
        /// <param name="httpClient">HTTP client used for API communication.</param>
        /// <param name="configuration">App configuration settings.</param>
        public SuggestionsController(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;

            if (_configuration["Environment"].Equals("Development"))
            {
                _httpClient.BaseAddress = new Uri("https://localhost:7280/api/suggestion/");
            }
            else
            {
                _httpClient.BaseAddress = new Uri("https://solarflowapi.azurewebsites.net/api/suggestion/");
            }
        }

        /// <summary>
        /// Generates new suggestions for the current user and redirects to the suggestions list view.
        /// </summary>
        /// <returns>Redirects to <see cref="GetPendingSuggestions"/>.</returns>
        public async Task<IActionResult> Index()
        {
            await GenerateSuggestions();
            return RedirectToAction("GetPendingSuggestions");
        }

        /// <summary>
        /// Sends a request to generate new suggestions for the authenticated user.
        /// </summary>
        /// <returns>
        /// HTTP 200 status code on success, or 401 Unauthorized if the user is not authenticated.
        /// </returns>
        [HttpPost]
        public async Task<IActionResult> GenerateSuggestions()
        {
            var token = Request.Cookies["AuthToken"];
            var userId = GetUserIdFromToken(token);

            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var requestMessage = new HttpRequestMessage(HttpMethod.Post, $"create/{userId}");
            requestMessage.Headers.Add("Authorization", $"Bearer {token}");
            await _httpClient.SendAsync(requestMessage);

            return StatusCode(200);
        }

        /// <summary>
        /// Retrieves a list of pending suggestions for the authenticated user.
        /// </summary>
        /// <returns>
        /// Renders the "Index" view with a list of suggestions,
        /// or returns 401 Unauthorized if the user is not authenticated.
        /// </returns>
        [HttpGet]
        public async Task<IActionResult> GetPendingSuggestions()
        {
            var token = Request.Cookies["AuthToken"];
            var userId = GetUserIdFromToken(token);

            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var requestMessage = new HttpRequestMessage(HttpMethod.Get, $"get/{userId}");
            requestMessage.Headers.Add("Authorization", $"Bearer {token}");

            var response = await _httpClient.SendAsync(requestMessage);
            var model = new GetSuggestionsViewModel();

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var suggestions = JsonConvert.DeserializeObject<List<SuggestionViewModel>>(json);
                model.Suggestions = suggestions;
            }

            return View("Index", model); 
        }

        /// <summary>
        /// Applies a specific suggestion for the user.
        /// </summary>
        /// <param name="suggestionId">The ID of the suggestion to apply.</param>
        /// <returns>Redirects to the pending suggestions view after applying.</returns>
        [HttpPost]
        public async Task<IActionResult> AcceptSuggestion(int suggestionId)
        {
            var token = Request.Cookies["AuthToken"];
            var request = new HttpRequestMessage(HttpMethod.Post, $"apply/{suggestionId}");
            request.Headers.Add("Authorization", $"Bearer {token}");

            await _httpClient.SendAsync(request);
            return RedirectToAction("GetPendingSuggestions");
        }

        /// <summary>
        /// Marks a specific suggestion as ignored.
        /// </summary>
        /// <param name="suggestionId">The ID of the suggestion to ignore.</param>
        /// <returns>Redirects to the pending suggestions view after ignoring.</returns>
        [HttpPost]
        public async Task<IActionResult> IgnoreSuggestion(int suggestionId)
        {
            var token = Request.Cookies["AuthToken"];
            var request = new HttpRequestMessage(HttpMethod.Post, $"ignore/{suggestionId}");
            request.Headers.Add("Authorization", $"Bearer {token}");

            await _httpClient.SendAsync(request);
            return RedirectToAction("GetPendingSuggestions");
        }

        /// <summary>
        /// Extracts the user ID (NameIdentifier claim) from a JWT token.
        /// </summary>
        /// <param name="token">The JWT token from the user's cookies.</param>
        /// <returns>The user ID as a string, or null if not found.</returns>
        private string GetUserIdFromToken(string token)
        {
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(token);
            return jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
        }
    }
}
