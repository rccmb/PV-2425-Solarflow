using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using SolarflowClient.Models.ViewModels.Authentication;
using SolarflowClient.Models;
using Microsoft.AspNetCore.Identity;
using System.IdentityModel.Tokens.Jwt;

namespace SolarflowClient.Controllers;

public class AuthenticationController : Controller
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;

    public AuthenticationController(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _configuration = configuration;

        if (_configuration["Environment"].Equals("Development"))
        {
            _httpClient.BaseAddress = new Uri("https://localhost:7280/api/auth/");
        }
        else
        {
            _httpClient.BaseAddress = new Uri("https://solarflowapi.azurewebsites.net/api/auth/");
        }
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

    // GET: Account Recovery form (to display the page)
    public IActionResult AccountRecovery()
    {
        return View();
    }
    
    // Controller Method for Forgot Password
    [HttpPost]
    public async Task<IActionResult> SubmitAccountRecovery(AccountRecoveryViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View("AccountRecovery", model);
        }

        var content = new StringContent(JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json");
        var response = await _httpClient.PostAsync("forgotpassword", content);

        if (response.IsSuccessStatusCode)
        {
            TempData["SuccessMessage"] = "If the email exists, a reset link has been sent.";
            return RedirectToAction("AccountRecovery");
        }

        var errorMessage = await response.Content.ReadAsStringAsync();
        ModelState.AddModelError(string.Empty, errorMessage);
        return View("AccountRecovery", model);
    }

    public IActionResult ResetPassword(string token)
    {
        if (string.IsNullOrEmpty(token))
        {
            ModelState.AddModelError("", "Invalid or expired password reset link.");
            return View();
        }

        var model = new ResetPasswordViewModel { Token = token};
        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> SubmitResetPassword(ResetPasswordViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var content = new StringContent(JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json");
        var response = await _httpClient.PostAsync("reset-password", content);

        if (response.IsSuccessStatusCode)
        {
            TempData["SuccessMessage"] = "Your password has been successfully reset!";
            return RedirectToAction("Login");
        }

        var errorMessage = await response.Content.ReadAsStringAsync();
        ModelState.AddModelError(string.Empty, errorMessage);
        return View("ResetPassword", model);
    }

    public IActionResult ConfirmEmail(string token, int userId)
    {
        if (string.IsNullOrEmpty(token))
        {
            ModelState.AddModelError("", "Invalid or expired email confirmation link.");
            return View();
        }
        var model = new ConfirmEmailViewModel { Token = token , UserId = userId.ToString()};
        return View(model);
    }

    public async Task<IActionResult> SubmitConfirmEmail(ConfirmEmailViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }
        var content = new StringContent(JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json");
        var response = await _httpClient.PostAsync("confirm-email", content);

        if (response.IsSuccessStatusCode)
        {
            TempData["SuccessMessage"] = "Your email has been successfully confirmed!";
            return RedirectToAction("Login");
        }

        var errorMessage = await response.Content.ReadAsStringAsync();
        ModelState.AddModelError(string.Empty, errorMessage);
        return View("ConfirmEmail", model);
    }

    [HttpPost]
    public async Task<IActionResult> ResendEmailConfirmation(ConfirmEmailViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var content = new StringContent(JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json");
        var response = await _httpClient.PostAsync("resend-email-confirmation", content);

        if (response.IsSuccessStatusCode)
        {
            TempData["SuccessMessage"] = "It's been sended a new confirmation email!";
            return RedirectToAction("Login");
        }

        var errorMessage = await response.Content.ReadAsStringAsync();
        ModelState.AddModelError(string.Empty, errorMessage);

        return View("ConfirmEmail", model);
    }
}
