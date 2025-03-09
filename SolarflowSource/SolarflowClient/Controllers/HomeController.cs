using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Newtonsoft.Json;
using SolarflowClient.Models.ViewModels.Authentication;
using SolarflowClient.Models;
using System.Net.Http.Headers;

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

       
        public IActionResult RegisterViewAccount()
        {
            return View("~/Views/Users/RegisterViewAccount.cshtml");
        }

        [HttpPost]
        public async Task<IActionResult> RegisterViewAccount(string Password)
        {
            var tokenJson = HttpContext.Session.GetString("AuthToken");
            var tokenObj = JsonConvert.DeserializeObject<dynamic>(tokenJson);
            var token = tokenObj?.token.ToString(); // Extraindo apenas o valor do token

            


            var requestData = new
            {
                Password
            };

            var jsonContent = new StringContent(JsonConvert.SerializeObject(requestData), Encoding.UTF8, "application/json");

            var request = new HttpRequestMessage(HttpMethod.Post, "auth/register-view")
            {
                Content = jsonContent
            };
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await _httpClient.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction("Index");
            }
            else
            {
                ViewBag.ErrorMessage = "Error registering the View Account.";
                return View("Index");
            }
        }
    }
}
