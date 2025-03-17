using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using SolarflowServer.DTOs.Authentication;
using SolarflowServer.Services;

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

    public AuthenticationController(
        UserManager<ApplicationUser> userManager, 
        SignInManager<ApplicationUser> signInManager,
        UserManager<ViewAccount> viewUserManager,
        SignInManager<ViewAccount> viewSignInManager,
        IConfiguration configuration, 
        IAuditService auditService)

    {
        _userManager = userManager;
        _signInManager = signInManager;
        _viewUserManager = viewUserManager;
        _viewSignInManager = viewSignInManager;
        _configuration = configuration;
        _auditService = auditService;
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


        var viewResult = await _viewSignInManager.PasswordSignInAsync(viewUser, model.Password, false, false);
        if (!viewResult.Succeeded)
        {
            return Unauthorized("Invalid credentials.");
        }

        var viewToken = GenerateJWTToken(viewUser);
        Console.WriteLine(viewToken);
            await _auditService.LogAsync(viewUser.Id.ToString(), viewUser.Email, "View user Logged In", GetClientIPAddress());
            return Ok(new { viewToken });

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

    private string GetClientIPAddress()
    {
        if (HttpContext?.Connection?.RemoteIpAddress == null)
        {
            return "127.0.0.1"; // Valor para testes
        }

        return HttpContext.Connection.RemoteIpAddress.ToString();
        }
    }
}

