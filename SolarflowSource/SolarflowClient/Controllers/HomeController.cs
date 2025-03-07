using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using SolarflowClient.Models.ViewModels.Authentication;
using SolarflowClient.Models;

namespace SolarflowClient.Controllers
{
    public class HomeController : Controller
    {
        private HttpClient _httpClient;

        public HomeController(HttpClient httpClient)
        {
            _httpClient = httpClient;
            _httpClient.BaseAddress = new Uri("https://localhost:7280/api/");
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            var token = HttpContext.Session.GetString("AuthToken");

            if (!string.IsNullOrEmpty(token))
            {
                var request = new HttpRequestMessage(HttpMethod.Post, "auth/logout");
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                await _httpClient.SendAsync(request);
            }

            HttpContext.Session.Remove("AuthToken");

            return RedirectToAction("Login", "Authentication");
        }
    }
}
