using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using SolarflowClient.Models.ViewModels.Authentication;
using SolarflowClient.Models;

namespace SolarflowClient.Controllers;

public class AuthenticationController : Controller
{
    private readonly HttpClient _httpClient;

    public AuthenticationController(HttpClient httpClient)
    {
        _httpClient = httpClient;
        // _httpClient.BaseAddress = new Uri("https://localhost:7280/api/auth/");
        _httpClient.BaseAddress = new Uri("https://solarflowapi.azurewebsites.net/api/auth/");
    }

    public IActionResult Register()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var content = new StringContent(JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json");
        var response = await _httpClient.PostAsync("register", content);

        if (response.IsSuccessStatusCode)
        {
            TempData["SuccessMessage"] = "Registration successful! You can now log in.";
            return RedirectToAction("Login");
        }

        var errorMessage = await response.Content.ReadAsStringAsync();

        var errors = JsonConvert.DeserializeObject<List<ErrorDetail>>(errorMessage);

        foreach (var error in errors)
        {
            ModelState.AddModelError(string.Empty, error.Description);
        }

        return View(model);
    }

    public IActionResult Login()
    {
        var token = Request.Cookies["AuthToken"];
        
        if (!string.IsNullOrEmpty(token))
        {
            return RedirectToAction("Index", "Home");
        }

        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var content = new StringContent(JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json");
        var response = await _httpClient.PostAsync("login", content);

        if (response.IsSuccessStatusCode)
        {
            var jsonResponse = await response.Content.ReadAsStringAsync();
            dynamic tokenResponse = JsonConvert.DeserializeObject(jsonResponse);
            string token = tokenResponse.token;

            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = false, 
                Expires = model.RememberMe ? DateTime.UtcNow.AddDays(30) : DateTime.UtcNow.AddHours(1)
            };

            Response.Cookies.Append("AuthToken", token, cookieOptions);

            Response.Cookies.Append("UserEmail", model.Email, cookieOptions);

            return RedirectToAction("Index", "Home");
        }

        var errorMessage = await response.Content.ReadAsStringAsync();
        ModelState.AddModelError(string.Empty, errorMessage);
        return View(model);
    }


    public IActionResult AccountRecovery()
    {
        return View();
    }
}
