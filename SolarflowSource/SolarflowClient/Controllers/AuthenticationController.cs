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

/// <summary>
/// Controller responsible for handling user authentication actions, including registration,
/// login, password recovery/reset, and email confirmation.
/// </summary>
public class AuthenticationController : Controller
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;

    /// <summary>
    /// Initializes a new instance of the <see cref="AuthenticationController"/> class.
    /// Sets the base address for the <see cref="HttpClient"/> depending on the environment.
    /// </summary>
    /// <param name="httpClient">The HTTP client used for API requests.</param>
    /// <param name="configuration">Application configuration to determine the environment.</param>
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

    /// <summary>
    /// Displays the registration view.
    /// </summary>
    /// <returns>The registration view.</returns>
    public IActionResult Register()
    {
        return View();
    }

    /// <summary>
    /// Submits the user registration form data to the authentication API.
    /// </summary>
    /// <param name="model">The registration data.</param>
    /// <returns>
    /// Redirects to login on success, or returns the same view with error messages on failure.
    /// </returns>
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

    /// <summary>
    /// Displays the login view. Redirects to the home page if the user is already authenticated.
    /// </summary>
    /// <returns>The login view or redirect to home.</returns>
    public IActionResult Login()
    {
        var token = Request.Cookies["AuthToken"];
        
        if (!string.IsNullOrEmpty(token))
        {
            return RedirectToAction("Index", "Home");
        }

        return View();
    }

    /// <summary>
    /// Submits the login form data to the authentication API and sets cookies if successful.
    /// </summary>
    /// <param name="model">The login credentials.</param>
    /// <returns>
    /// Redirects to the home page on success, or returns the login view with error messages.
    /// </returns>
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

    /// <summary>
    /// Displays the account recovery (forgot password) form.
    /// </summary>
    /// <returns>The account recovery view.</returns>
    public IActionResult AccountRecovery()
    {
        return View();
    }

    /// <summary>
    /// Submits the account recovery request to the authentication API.
    /// </summary>
    /// <param name="model">The account recovery form data.</param>
    /// <returns>
    /// Redirects with success message if the email exists; otherwise, returns view with error.
    /// </returns>
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

    /// <summary>
    /// Displays the reset password view using the provided token.
    /// </summary>
    /// <param name="token">The password reset token.</param>
    /// <returns>The reset password view with token pre-filled.</returns>
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

    /// <summary>
    /// Submits the new password along with token to reset the user password.
    /// </summary>
    /// <param name="model">The reset password form data.</param>
    /// <returns>
    /// Redirects to login on success, or returns view with error messages.
    /// </returns>
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

    /// <summary>
    /// Displays the email confirmation page using the token and user ID.
    /// </summary>
    /// <param name="token">The email confirmation token.</param>
    /// <param name="userId">The ID of the user.</param>
    /// <returns>The confirm email view with token and user ID.</returns>
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

    /// <summary>
    /// Submits the email confirmation token and user ID to the authentication API.
    /// </summary>
    /// <param name="model">The email confirmation data.</param>
    /// <returns>
    /// Redirects to login on success, or returns the confirm email view with errors.
    /// </returns>
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

    /// <summary>
    /// Requests a new confirmation email to be sent to the user.
    /// </summary>
    /// <param name="model">The email confirmation view model containing user data.</param>
    /// <returns>
    /// Redirects to login on success, or returns the confirm email view with error messages.
    /// </returns>
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
