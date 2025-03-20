using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using SolarflowServer.DTOs.Authentication;
using SolarflowServer.Services;
<<<<<<< HEAD
=======
using static Org.BouncyCastle.Math.EC.ECCurve;
using Microsoft.AspNetCore.Identity.Data;
using static System.Net.WebRequestMethods;
>>>>>>> Email-Confirmation
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
<<<<<<< HEAD

        private readonly ApplicationDbContext _context;

        public AuthenticationController(
            UserManager<ApplicationUser> userManager, 
            SignInManager<ApplicationUser> signInManager,
            UserManager<ViewAccount> viewUserManager,
            SignInManager<ViewAccount> viewSignInManager,
            IConfiguration configuration, 
            IAuditService auditService,
            ApplicationDbContext context)
=======
        private readonly IEmailSender _emailSender;


        public AuthenticationController(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        UserManager<ViewAccount> viewUserManager,
        SignInManager<ViewAccount> viewSignInManager,
        IConfiguration configuration,
        IAuditService auditService,
        IEmailSender emailSender)
>>>>>>> Email-Confirmation

        {
            _userManager = userManager;
            _signInManager = signInManager;
            _viewUserManager = viewUserManager;
            _viewSignInManager = viewSignInManager;
            _configuration = configuration;
            _auditService = auditService;
<<<<<<< HEAD
            _context = context;
=======
            _emailSender = emailSender;
>>>>>>> Email-Confirmation
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDTO model)
        {
            var user = new ApplicationUser
<<<<<<< HEAD
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

            return Ok(new { message = "User registered successfully!" });
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
            if (user != null)
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
=======
            {
                UserName = model.Email, // Do not remove.
                Email = model.Email, // Do not remove.
                Fullname = model.Fullname,
                Photo = "",
                ConfirmedEmail = false,
                BatteryAPI = "",
                CreatedAt = DateTime.UtcNow
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (!result.Succeeded)
                return BadRequest(result.Errors);

            await _auditService.LogAsync(user.Id.ToString(), user.Email, "User Registered", GetClientIPAddress());

            return Ok(new { message = "User registered successfully!" });
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
            if (user == null)
                return BadRequest("User not found.");


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


            if (!viewResult.Succeeded)
                return BadRequest(viewResult.Errors);

            user.ViewAccount = viewAccount;
            await _auditService.LogAsync(viewAccount.Id.ToString(), user.Email, "View user Registered", GetClientIPAddress());
            return Ok(new { message = "ViewAccount registered successfully!" });
        }


        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDTO model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);

            if (user != null)
            {
                var result = await _signInManager.PasswordSignInAsync(user, model.Password, false, false);
                if (result.Succeeded)
                {
                    var token = GenerateJWTToken(user);
                    Console.WriteLine(token);
                    await _auditService.LogAsync(user.Id.ToString(), user.Email, "User Logged In", GetClientIPAddress());
                    return Ok(new { token });
                }
            }

            var viewUser = await _viewUserManager.FindByEmailAsync(model.Email);
            if (viewUser == null)
            {
                return Unauthorized("Invalid credentials.");
            }
>>>>>>> Email-Confirmation

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
<<<<<<< HEAD
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
=======
            Console.WriteLine(viewToken);
            await _auditService.LogAsync(viewUser.Id.ToString(), viewUser.Email, "View user Logged In", GetClientIPAddress());
            return Ok(new { viewToken });

        }


>>>>>>> Email-Confirmation


        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
<<<<<<< HEAD

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

        private string GetClientIPAddress()
        {
            if (HttpContext?.Connection?.RemoteIpAddress == null)
            {
                return "127.0.0.1"; // Valor para testes
            }

            return HttpContext.Connection.RemoteIpAddress.ToString();
=======

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

            var message = new Message(new string[] { model.Email }, "Password Reset Link", "https://www.youtube.com/");

            // Send the email
            await _emailSender.SendEmailAsync(message);

            // Return success response
            return Ok(new { message = "If the email exists, a reset link has been sent." });
>>>>>>> Email-Confirmation
        }




    }
}

