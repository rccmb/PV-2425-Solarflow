using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using SolarflowServer.DTOs.Authentication;

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

    public AuthenticationController(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        UserManager<ViewAccount> viewUserManager,
        SignInManager<ViewAccount> viewSignInManager,
        IConfiguration configuration)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _viewUserManager = viewUserManager;
        _viewSignInManager = viewSignInManager;
        _configuration = configuration;
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

        return Ok(new { message = "User registered successfully!" });
    }

    [HttpPost("register-view")]
    public async Task<IActionResult> RegisterViewAccount([FromBody] RegisterViewDTO model)
    {
        // Verificar se o usuário principal existe
        var user = await _userManager.FindByIdAsync(model.UserId.ToString());
        if (user == null)
            return BadRequest("User not found.");

        // Verificar se já existe uma ViewAccount associada
        if (await _viewUserManager.FindByEmailAsync(user.Email) != null)
            return BadRequest("A ViewAccount is already registered for this user.");

        // Criar a ViewAccount associada
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
        return Ok(new { viewToken });
    }




        [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        return Ok(new { message = "User logged out successfully!" });
    }

    private string GenerateJWTToken(IdentityUser<int> user)
    {
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email)
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Secret"]));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            claims: claims,
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
}

