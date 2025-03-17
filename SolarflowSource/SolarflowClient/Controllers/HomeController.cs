using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Newtonsoft.Json;
using SolarflowClient.Models.ViewModels.Authentication;
using SolarflowClient.Models;
using Microsoft.AspNetCore.Authorization;
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
            var token = Request.Cookies["AuthToken"];

            if (string.IsNullOrEmpty(token))
            {
                return RedirectToAction("Login", "Authentication");
            }

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            var token = Request.Cookies["AuthToken"];

            if (!string.IsNullOrEmpty(token))
            {
                var request = new HttpRequestMessage(HttpMethod.Post, "auth/logout");
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                await _httpClient.SendAsync(request);
            }

            Response.Cookies.Delete("AuthToken");

            return RedirectToAction("Login", "Authentication");
        }


        public IActionResult AccessDenied()
        {
            return View();
        }
       
        public IActionResult RegisterViewAccount()
        {
            return View("~/Views/Users/RegisterViewAccount.cshtml");
        }

        [HttpPost]
        public async Task<IActionResult> RegisterViewAccount(string Password)
        {
            var token = Request.Cookies["AuthToken"];


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
