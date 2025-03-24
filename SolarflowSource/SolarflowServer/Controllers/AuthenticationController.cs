using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using SolarflowServer.DTOs.Authentication;
using SolarflowServer.DTOs.Settings;
using SolarflowServer.Services;
using static Org.BouncyCastle.Math.EC.ECCurve;
using Microsoft.AspNetCore.Identity.Data;
using static System.Net.WebRequestMethods;
using SolarflowServer.Models;

namespace SolarflowServer.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class AuthenticationController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ViewAccount> _viewUserManager;
        private readonly SignInManager<ViewAccount> _viewSignInManager;
        private readonly IConfiguration _configuration;
        private readonly IAuditService _auditService;
		private readonly IEmailSender _emailSender;
        private readonly ApplicationDbContext _context;

        public AuthenticationController(
			UserManager<ApplicationUser> userManager,
			SignInManager<ApplicationUser> signInManager,
			UserManager<ViewAccount> viewUserManager,
			SignInManager<ViewAccount> viewSignInManager,
			IConfiguration configuration,
			IAuditService auditService,
			IEmailSender emailSender,
            ApplicationDbContext context
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
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDTO model)
        {
            var user = new ApplicationUser
            {
                UserName = model.Email, // Do not remove.
                Email = model.Email, // Do not remove.
                Fullname = model.Fullname,
                Photo = "",
                ConfirmedEmail = false,
                CreatedAt = DateTime.UtcNow
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (!result.Succeeded) return BadRequest(result.Errors);

            var battery = new Battery
            {
                User = user, 
                ChargeLevel = 40,
                ChargingSource = "Solar",
                BatteryMode = "Personalized",
                MinimalTreshold = 0,
                MaximumTreshold = 100,
                SpendingStartTime = "00:00",
                SpendingEndTime = "09:00",
                LastUpdate = DateTime.UtcNow.ToString()
            };

            _context.Batteries.Add(battery);
            await _context.SaveChangesAsync(); 

            await _auditService.LogAsync(user.Id.ToString(), user.Email, "User Registered", GetClientIPAddress());

            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);

            var baseUrl = _configuration["BaseUrl"]; // Get the base URL from config
            var resetLink = $"{baseUrl}Authentication/ConfirmEmail?token={Uri.EscapeDataString(token)}&userId={Uri.EscapeDataString(user.Id.ToString())}";

            var message = new Message(
                new string[] { model.Email },
                "Password Reset Link",
                $"Click <a href='{resetLink}'>here</a> to confirm your email."
            );

            // Send the email
            await _emailSender.SendEmailAsync(message);

            return Ok(new { message = "User registered successfully!" });
        }

        [HttpPost("resend-email-confirmation")]
        public async Task<IActionResult> ResendEmailConfirmation([FromBody] ConfirmEmailDTO model)
        {
            var user = await _userManager.FindByIdAsync(model.UserId);
            if (user == null)
            {
                return BadRequest(new { message = "Invalid email." });
            }
            if (user.ConfirmedEmail)
            {
                return BadRequest(new { message = "Email already confirmed." });
            }
            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var baseUrl = _configuration["BaseUrl"]; // Get the base URL from config
            var resetLink = $"{baseUrl}Authentication/ConfirmEmail?token={Uri.EscapeDataString(token)}&userId={Uri.EscapeDataString(user.Id.ToString())}";
            var message = new Message(
                new string[] { user.Email },
                "Password Reset Link",
                $"Click <a href='{resetLink}'>here</a> to confirm your email."
            );
            // Send the email
            await _emailSender.SendEmailAsync(message);
            return Ok(new { message = "Email confirmation link sent successfully!" });
        }

        [HttpPost("register-view")]
        public async Task<IActionResult> RegisterViewAccount([FromBody] RegisterViewDTO model)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
          

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }
           
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null) return BadRequest("User not found.");
        
            if (await _viewUserManager.FindByEmailAsync(user.Email) != null) return BadRequest("A ViewAccount is already registered for this user.");

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
            await _auditService.LogAsync(viewAccount.Id.ToString(), user.Email, "View user Registered", GetClientIPAddress());
            return Ok(new { message = "ViewAccount registered successfully!" });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDTO model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user != null && user.EmailConfirmed == true)
            {
                var result = await _signInManager.PasswordSignInAsync(user, model.Password, false, false);
                if (result.Succeeded)
                {
                    var token = GenerateJWTToken(user);
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
            }

            if(user != null && user.EmailConfirmed == false)
            {
                return Unauthorized("Email not confirmed.");
            }

            var viewUser = await _viewUserManager.FindByEmailAsync(model.Email);
            if (viewUser == null)
            {
                return Unauthorized("Invalid credentials.");
            }

            var viewResult = await _viewSignInManager.PasswordSignInAsync(viewUser, model.Password, false, false);
            if (!viewResult.Succeeded)
            {
                return Unauthorized("Invalid credentials.");
            }

            var viewToken = GenerateJWTToken(viewUser);
            var viewCookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Expires = DateTime.UtcNow.AddHours(1),
                Secure = true,
                SameSite = SameSiteMode.Strict
            };
            Response.Cookies.Append("AuthToken", viewToken, viewCookieOptions);
            await _auditService.LogAsync(viewUser.Id.ToString(), viewUser.Email, "View user Logged In", GetClientIPAddress());
            return Ok(new { token = viewToken });
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var email = User.FindFirstValue(ClaimTypes.Email);

            if (userId != null)
                await _auditService.LogAsync(userId, email, "User Logged Out", GetClientIPAddress());

            return Ok(new { message = "User logged out successfully!" });
        }

        private string GenerateJWTToken(IdentityUser<int> user)
        {
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Secret"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.UtcNow.AddHours(1),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        [HttpPost("confirm-email")]
        public async Task<IActionResult> ConfirmEmail([FromBody] ConfirmEmailDTO model)
        {
            if (string.IsNullOrEmpty(model.UserId) || string.IsNullOrEmpty(model.Token))
            {
                return BadRequest(new { message = "Invalid request." });
            }
            var user = await _userManager.FindByIdAsync(model.UserId);
            if (user == null)
            {
                return BadRequest(new { message = "Invalid user." });
            }

            var result = await _userManager.ConfirmEmailAsync(user, model.Token);
            if (!result.Succeeded)
            {
                return BadRequest(new { message = "Invalid request." });
            }
            return Ok(new { message = "Email confirmed successfully!" });
        }

        private string GetClientIPAddress()
        {
            if (HttpContext?.Connection?.RemoteIpAddress == null)
            {
                return "127.0.0.1"; // Valor para testes
            }

            return HttpContext.Connection.RemoteIpAddress.ToString();
        }

        [HttpPost("forgotpassword")]
        public async Task<IActionResult> ForgotPassword([FromBody] AccountRecoveryViewModel model)
        {
            // Validate model
            if (!ModelState.IsValid || string.IsNullOrEmpty(model.Email))
            {
                return BadRequest(new { message = "Invalid email format." });
            }

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                return Ok(new { message = "If the email exists, a reset link has been sent." });
            }

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);

            var baseUrl = _configuration["BaseUrl"]; // Get the base URL from config
            var resetLink = $"{baseUrl}Authentication/ResetPassword?token={Uri.EscapeDataString(token)}";

            var message = new Message(
                new string[] { model.Email },
                "Password Reset Link",
                $"Click <a href='{resetLink}'>here</a> to reset your password."
            );

            // Send the email
            await _emailSender.SendEmailAsync(message);

            // Return success response
            return Ok(new { message = "If the email exists, a reset link has been sent." });
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDTO model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { message = "Invalid request data." });
            }

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                return BadRequest(new { message = "Invalid reset request." });
            }

            var result = await _userManager.ResetPasswordAsync(user, model.Token, model.NewPassword);
            if (!result.Succeeded)
            {
                return BadRequest(result.Errors);
            }

            return Ok(new { message = "Password reset successfully!" });
        }



        [HttpGet("get-user")]
        public async Task<IActionResult> GetUser()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userId == null)
                return Unauthorized(new { error = "User not authenticated." });

            if (!int.TryParse(userId, out var parsedUserId))
                return BadRequest(new { error = "Invalid user ID" });

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
                HasViewAccount = viewAccount != null
            };

            return Ok(userDTO);
        }

        [HttpPost("update-user")]
        public async Task<IActionResult> UpdateUser([FromBody] ChangeUserDTO model)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userId == null)
                return Unauthorized(new { error = "User not authenticated." });

            if (!int.TryParse(userId, out var parsedUserId))
                return BadRequest(new { error = "Invalid user ID" });
            var user = await _userManager.FindByIdAsync(parsedUserId.ToString());

            if (user == null)
                return NotFound(new { error = "User not found." });

            user.Fullname = model.Fullname;

            await _userManager.UpdateAsync(user);

            return Ok(new { message = "User updated successfully!" });
        }

        [HttpPost("delete-user-view-model")]
        public async Task<IActionResult> DeleteUserViewModel()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userId == null)
                return Unauthorized(new { error = "User not authenticated." });

            if (!int.TryParse(userId, out var parsedUserId))
                return BadRequest(new { error = "Invalid user ID" });

            var user = await _userManager.FindByIdAsync(parsedUserId.ToString());

            if (user == null)
                return NotFound(new { error = "User not found." });

            var viewAccount = await _viewUserManager.FindByEmailAsync(user.Email);

            if (viewAccount == null)
                return NotFound(new { error = "View Account not found." });

            var result = await _viewUserManager.DeleteAsync(viewAccount);

            if (!result.Succeeded)
                return BadRequest(new { error = "An error occurred while deleting the View Account." });

            return Ok(new { message = "View Account deleted successfully!" });
        }
    }
}

