using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using SolarflowClient.Models.ViewModels.Suggestions;

namespace SolarflowClient.Controllers
{
    public class SuggestionsController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        public SuggestionsController(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;

            if (_configuration["Environment"].Equals("Development"))
            {
                _httpClient.BaseAddress = new Uri("https://localhost:7280/api/suggestions/");
            }
            else
            {
                _httpClient.BaseAddress = new Uri("https://solarflowapi.azurewebsites.net/api/suggestions/");
            }
        }

        
        [HttpPost]
        public async Task<IActionResult> GenerateSuggestions()
        {
            var token = Request.Cookies["AuthToken"];
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(token);

            var userId = jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

           
            var requestMessage = new HttpRequestMessage(HttpMethod.Post, "create");
            requestMessage.Headers.Add("Authorization", $"Bearer {token}");

            var response = await _httpClient.SendAsync(requestMessage);
            if (!response.IsSuccessStatusCode)
            {
                TempData["ErrorMessage"] = "Erro ao gerar as sugestões.";
                return RedirectToAction("Index", "Home");
            }

            return RedirectToAction("Index", "Home");
        }

       
        [HttpGet]
        public async Task<IActionResult> GetPendingSuggestions()
        {
            var token = Request.Cookies["AuthToken"];
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(token);

            var userId = jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var requestMessage = new HttpRequestMessage(HttpMethod.Get, $"get/{userId}");
            requestMessage.Headers.Add("Authorization", $"Bearer {token}");

            var response = await _httpClient.SendAsync(requestMessage);
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var suggestions = JsonConvert.DeserializeObject<List<GetSuggestionsViewModel>>(json);
                return PartialView("_SuggestionPopup", suggestions);
            }

            return PartialView("_SuggestionPopup", new List<GetSuggestionsViewModel>());
        }

    
        [HttpPost]
        public async Task<IActionResult> AcceptSuggestion(int suggestionId)
        {
            var token = Request.Cookies["AuthToken"];
            var request = new HttpRequestMessage(HttpMethod.Post, $"apply/{suggestionId}");
            request.Headers.Add("Authorization", $"Bearer {token}");

            await _httpClient.SendAsync(request);
            return RedirectToAction("Index", "Home");
        }

       
        [HttpPost]
        public async Task<IActionResult> IgnoreSuggestion(int suggestionId)
        {
            var token = Request.Cookies["AuthToken"];
            var request = new HttpRequestMessage(HttpMethod.Post, $"ignore/{suggestionId}");
            request.Headers.Add("Authorization", $"Bearer {token}");

            await _httpClient.SendAsync(request);
            return RedirectToAction("Index", "Home");
        }
    }
}
