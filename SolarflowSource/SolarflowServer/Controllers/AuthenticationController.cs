using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using SolarflowServer.DTOs.Authentication;
using SolarflowServer.DTOs.Settings;
using SolarflowServer.Models;
using SolarflowServer.Services;
using SolarflowServer.Services.Interfaces;

namespace SolarflowServer.Controllers;

/// <summary>
///     Controller responsible for user authentication, registration, login, and account management operations.
/// </summary>
[Route("api/auth")]
[ApiController]
public class AuthenticationController : ControllerBase
{
    private readonly IAuditService _auditService;
    private readonly IConfiguration _configuration;
    private readonly ApplicationDbContext _context;
    private readonly DemoService _demoService;
    private readonly EmailSender _emailSender;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ViewAccount> _viewSignInManager;
    private readonly UserManager<ViewAccount> _viewUserManager;

    /// <summary>
    ///     Initializes a new instance of the <see cref="AuthenticationController" /> class.
    /// </summary>
    /// <param name="userManager">The user manager.</param>
    /// <param name="signInManager">The sign-in manager.</param>
    /// <param name="viewUserManager">The view account user manager.</param>
    /// <param name="viewSignInManager">The view account sign-in manager.</param>
    /// <param name="configuration">The application configuration.</param>
    /// <param name="auditService">The audit service.</param>
    /// <param name="emailSender">The email sender service.</param>
    /// <param name="context">The database context.</param>
    public AuthenticationController(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        UserManager<ViewAccount> viewUserManager,
        SignInManager<ViewAccount> viewSignInManager,
        IConfiguration configuration,
        IAuditService auditService,
        EmailSender emailSender,
        ApplicationDbContext context,
        DemoService demoService
    )
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _viewUserManager = viewUserManager;
        _viewSignInManager = viewSignInManager;
        _configuration = configuration;
        _auditService = auditService;
        _context = context;
        _emailSender = emailSender;
        _demoService = demoService;
    }

    /// <summary>
    ///     Registers a new user in the system.
    /// </summary>
    /// <param name="model">The registration model containing user details.</param>
    /// <returns>A result indicating the success or failure of the registration.</returns>
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDTO model)
    {
        var random = new Random();

        var user = new ApplicationUser
        {
            UserName = model.Email, // Do not remove.
            Email = model.Email, // Do not remove.
            Fullname = model.Fullname,
            Photo = "",
            ConfirmedEmail = false,
            CreatedAt = DateTime.UtcNow,
            GridKWh = 10.35,
            SolarKWh = 3.5,
            Latitude = Math.Round(model.Latitude ?? random.NextDouble() * (70 - 35) + 35, 4),
            Longitude = Math.Round(model.Longitude ?? random.NextDouble() * (40 - -10) + -10, 4)
        };

        var result = await _userManager.CreateAsync(user, model.Password);

        if (!result.Succeeded) return BadRequest(result.Errors);

        var battery = new Battery
        {
            User = user
        };

        _context.Batteries.Add(battery);
        await _context.SaveChangesAsync();


        const int daysAgo = 7;
        const int minutes = 15;
        var start = DateTime.Now.AddDays(-daysAgo);
        var now = DateTime.Now;
        while (start <= now)
        {
            await _demoService.DemoEnergyIteration(user.Id, minutes, start);
            start = start.AddMinutes(minutes);
        }


        // Confirmation Link
        var baseUrlClient = _configuration["BaseUrlClient"];
        var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
        var confirmationLink =
            $"{baseUrlClient}Authentication/ConfirmEmail?token={Uri.EscapeDataString(token)}&userId={Uri.EscapeDataString(user.Id.ToString())}";


        // Create & Send Email
        const string subject = "Confirmation Link";
        var body = $"Click <a href='{confirmationLink}'>here</a> to confirm your email.";
        var message = _emailSender.CreateMessage([model.Email], subject, body, true);
        await _emailSender.SendMessage(message);


        // Add Log Entry
        await _auditService.LogAsync(user.Id.ToString(), "Account Creation", "New User Registered",
            GetClientIPAddress());

        return Ok(new { message = "User registered successfully!" });
    }

    /// <summary>
    ///     Resends the email confirmation link to the user.
    /// </summary>
    /// <param name="model">The model containing user information for email confirmation.</param>
    /// <returns>A result indicating the success or failure of resending the confirmation email.</returns>
    [HttpPost("resend-email-confirmation")]
    public async Task<IActionResult> ResendEmailConfirmation([FromBody] ConfirmEmailDTO model)
    {
        var user = await _userManager.FindByIdAsync(model.UserId);
        if (user == null) return BadRequest(new { message = "Invalid email." });
        if (user.ConfirmedEmail) return BadRequest(new { message = "Email already confirmed." });

        // Confirmation Link
        var baseUrlClient = _configuration["BaseUrlClient"];
        var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
        var confirmationLink =
            $"{baseUrlClient}Authentication/ConfirmEmail?token={Uri.EscapeDataString(token)}&userId={Uri.EscapeDataString(user.Id.ToString())}";

        // Create & Send Email
        const string subject = "Confirmation Link";
        var body = $"Click <a href='{confirmationLink}'>here</a> to confirm your email.";
        var message = _emailSender.CreateMessage([user.Email], subject, body, true);
        await _emailSender.SendMessage(message);


        // Add Log Entry
        // await _auditService.LogAsync(user.Id.ToString(), "Email Confirmation", "Resent Email Confirmation", GetClientIPAddress());

        return Ok(new { message = "Email confirmation link sent successfully!" });
    }

    /// <summary>
    ///     Registers a view account for an existing user.
    /// </summary>
    /// <param name="model">The view account registration model.</param>
    /// <returns>A result indicating the success or failure of the view account registration.</returns>
    [HttpPost("register-view")]
    public async Task<IActionResult> RegisterViewAccount([FromBody] RegisterViewDTO model)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);


        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) return BadRequest("User not found.");

        if (await _viewUserManager.FindByEmailAsync(user.Email) != null)
            return BadRequest("A ViewAccount is already registered for this user.");

        var viewAccount = new ViewAccount
        {
            UserName = user.Email,
            Email = user.Email,
            Name = $"{user.Fullname}'s View Account",
            UserId = user.Id
        };

        var viewResult = await _viewUserManager.CreateAsync(viewAccount, model.Password);

        if (!viewResult.Succeeded) return BadRequest(viewResult.Errors);

        user.ViewAccount = viewAccount;

        // await _auditService.LogAsync(viewAccount.Id.ToString(), "View Account", "View Account Registered", GetClientIPAddress());
        return Ok(new { message = "ViewAccount registered successfully!" });
    }

    /// <summary>
    ///     Logs in a user by generating a JWT token.
    /// </summary>
    /// <param name="model">The login model containing user credentials.</param>
    /// <returns>A result containing the JWT token if login is successful.</returns>
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDTO model)
    {
        var user = await _userManager.FindByEmailAsync(model.Email);
        if (user != null && user.EmailConfirmed)
        {
            var result = await _signInManager.PasswordSignInAsync(user, model.Password, false, false);
            if (result.Succeeded)
            {
                var token = GenerateJWTToken(user, "Admin");
                var cookieOptions = new CookieOptions
                {
                    HttpOnly = true,
                    Expires = DateTime.UtcNow.AddHours(1),
                    Secure = true,
                    SameSite = SameSiteMode.Strict
                };

                Response.Cookies.Append("AuthToken", token, cookieOptions);

                await _auditService.LogAsync(user.Id.ToString(), user.Email, "User Logged In", GetClientIPAddress());
                return Ok(new { token });
            }

            if (user != null && user.EmailConfirmed == false) return Unauthorized("Email not confirmed.");

            var viewUser = await _viewUserManager.FindByEmailAsync(model.Email);
            if (viewUser == null) return Unauthorized("Invalid credentials.");

            var viewResult = await _viewSignInManager.PasswordSignInAsync(viewUser, model.Password, false, false);
            if (!viewResult.Succeeded) return Unauthorized("Invalid credentials.");

            var viewToken = GenerateJWTToken(viewUser, "View");
            var viewCookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Expires = DateTime.UtcNow.AddHours(1),
                Secure = true,
                SameSite = SameSiteMode.Strict
            };
            Response.Cookies.Append("AuthToken", viewToken, viewCookieOptions);

            await _auditService.LogAsync(viewUser.Id.ToString(), viewUser.Email, "View user Logged In",
                GetClientIPAddress());
            return Ok(new { token = viewToken });
        }

        return Unauthorized("Confirm Email.");
    }

    /// <summary>
    ///     Logs out the currently authenticated user.
    /// </summary>
    /// <returns>A result indicating the success or failure of the logout operation.</returns>
    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var email = User.FindFirstValue(ClaimTypes.Email);

        // if (userId != null) await _auditService.LogAsync(userId, "Authentication", "User Logged Out", GetClientIPAddress());

        return Ok(new { message = "User logged out successfully!" });
    }

    /// <summary>
    ///     Generates a JWT token for the specified user and role.
    /// </summary>
    /// <param name="user">The user for whom the token will be generated.</param>
    /// <param name="role">The role of the user (e.g., "Admin" or "View").</param>
    /// <returns>The generated JWT token as a string.</returns>
    private string GenerateJWTToken(IdentityUser<int> user, string role)
    {
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Role, role), // ? Role claim is correct
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(JwtRegisteredClaimNames.Exp,
                ((DateTimeOffset)DateTime.UtcNow.AddHours(1)).ToUnixTimeSeconds().ToString())
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Secret"]));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            _configuration["Jwt:Issuer"],
            _configuration["Jwt:Audience"],
            claims,
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    /// <summary>
    ///     Confirms the email of the user by verifying the provided token.
    /// </summary>
    /// <param name="model">The model containing the user id and confirmation token.</param>
    /// <returns>A result indicating the success or failure of the email confirmation.</returns>
    [HttpPost("confirm-email")]
    public async Task<IActionResult> ConfirmEmail([FromBody] ConfirmEmailDTO model)
    {
        if (string.IsNullOrEmpty(model.UserId) || string.IsNullOrEmpty(model.Token))
            return BadRequest(new { message = "Invalid request." });
        var user = await _userManager.FindByIdAsync(model.UserId);
        if (user == null) return BadRequest(new { message = "Invalid user." });

        var result = await _userManager.ConfirmEmailAsync(user, model.Token);
        if (!result.Succeeded) return BadRequest(new { message = "Invalid request." });

        // await _auditService.LogAsync(user.Id.ToString(), "Authentication", "Email Confirmed", GetClientIPAddress());
        return Ok(new { message = "Email confirmed successfully!" });
    }

    /// <summary>
    ///     Initiates the password recovery process by sending a reset link to the provided email.
    /// </summary>
    /// <param name="model">The account recovery model containing the email address.</param>
    /// <returns>A result indicating whether a reset link was sent or not.</returns>
    [HttpPost("forgotpassword")]
    public async Task<IActionResult> ForgotPassword([FromBody] AccountRecoveryViewModel model)
    {
        // Validate model
        if (!ModelState.IsValid || string.IsNullOrEmpty(model.Email))
            return BadRequest(new { message = "Invalid email format." });

        var user = await _userManager.FindByEmailAsync(model.Email);
        if (user == null) return Ok(new { message = "If the email exists, a reset link has been sent." });

        // Confirmation Link
        var baseUrlClient = _configuration["BaseUrlClient"];
        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        var resetLink = $"{baseUrlClient}Authentication/ResetPassword?token={Uri.EscapeDataString(token)}";

        // Create & Send Email
        const string subject = "Password Reset Link";
        var body = $"Click <a href='{resetLink}'>here</a> to reset your password.";
        var message = _emailSender.CreateMessage([model.Email], subject, body, true);
        await _emailSender.SendMessage(message);

        // Add Log Entry
        // await _auditService.LogAsync(user.Id.ToString(), "Authentication", "Forgot Password Requested", GetClientIPAddress());

        return Ok(new { message = "If the email exists, a reset link has been sent." });
    }

    /// <summary>
    ///     Resets the user's password using the provided reset token and new password.
    /// </summary>
    /// <param name="model">The model containing the email, token, and new password.</param>
    /// <returns>A result indicating whether the password was successfully reset or not.</returns>
    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDTO model)
    {
        if (!ModelState.IsValid) return BadRequest(new { message = "Invalid request data." });

        var user = await _userManager.FindByEmailAsync(model.Email);
        if (user == null) return BadRequest(new { message = "Invalid reset request." });

        var result = await _userManager.ResetPasswordAsync(user, model.Token, model.NewPassword);
        if (!result.Succeeded) return BadRequest(result.Errors);

        // await _auditService.LogAsync(user.Id.ToString(), "Authentication", "Password Reset", GetClientIPAddress());
        return Ok(new { message = "Password reset successfully!" });
    }

    /// <summary>
    ///     Retrieves the current authenticated user's details.
    /// </summary>
    /// <returns>A result containing the user details.</returns>
    [HttpGet("get-user")]
    public async Task<IActionResult> GetUser()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (userId == null)
            return Unauthorized(new { error = "User not authenticated." });

        if (!int.TryParse(userId, out var parsedUserId))
            return BadRequest(new { error = "Invalid user Id" });

        var user = await _userManager.FindByIdAsync(parsedUserId.ToString());

        if (user == null)
            return NotFound(new { error = "User not found." });

        var viewAccount = await _viewUserManager.FindByEmailAsync(user.Email);

        var userDTO = new GetUserDTO
        {
            Fullname = user.Fullname,
            Email = user.Email,
            Photo = user.Photo,
            CreatedAt = user.CreatedAt,
            HasViewAccount = viewAccount != null,
            GridKWh = user.GridKWh,
            SolarKWh = user.SolarKWh,
            Latitude = user.Latitude,
            Longitude = user.Longitude
        };

        return Ok(userDTO);
    }

    /// <summary>
    ///     Updates the details of the current authenticated user.
    /// </summary>
    /// <param name="model">The model containing the updated user data.</param>
    /// <returns>A result indicating the success or failure of the user update.</returns>
    [HttpPost("update-user")]
    public async Task<IActionResult> UpdateUser([FromBody] ChangeUserDTO model)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (userId == null)
            return Unauthorized(new { error = "User not authenticated." });

        if (!int.TryParse(userId, out var parsedUserId))
            return BadRequest(new { error = "Invalid user Id" });
        var user = await _userManager.FindByIdAsync(parsedUserId.ToString());

        if (user == null)
            return NotFound(new { error = "User not found." });

        user.Fullname = model.Fullname;
        user.GridKWh = model.GridKWh;
        user.Latitude = model.Latitude;
        user.Longitude = model.Longitude;
        user.SolarKWh = model.SolarKWh;

        await _userManager.UpdateAsync(user);

        // await _auditService.LogAsync(user.Id.ToString(), "User Access", "User Data Updated", GetClientIPAddress());
        return Ok(new { message = "User updated successfully!" });
    }

    /// <summary>
    ///     Deletes the View Account associated with the currently authenticated user.
    /// </summary>
    /// <returns>A result indicating the success or failure of the View Account deletion process.</returns>
    [HttpPost("delete-user-view-model")]
    public async Task<IActionResult> DeleteUserViewModel()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (userId == null)
            return Unauthorized(new { error = "User not authenticated." });

        if (!int.TryParse(userId, out var parsedUserId))
            return BadRequest(new { error = "Invalid user Id" });

        var user = await _userManager.FindByIdAsync(parsedUserId.ToString());

        if (user == null)
            return NotFound(new { error = "User not found." });

        var viewAccount = await _viewUserManager.FindByEmailAsync(user.Email);

        if (viewAccount == null)
            return NotFound(new { error = "View Account not found." });

        var result = await _viewUserManager.DeleteAsync(viewAccount);

        if (!result.Succeeded)
            return BadRequest(new { error = "An error occurred while deleting the View Account." });

        // await _auditService.LogAsync(user.Id.ToString(), "View Account", "View Account Deleted", GetClientIPAddress());
        return Ok(new { message = "View Account deleted successfully!" });
    }

    private string GetClientIPAddress()
    {
        return HttpContext?.Connection?.RemoteIpAddress == null
            ? "127.0.0.1"
            : HttpContext.Connection.RemoteIpAddress.ToString();
    }
}