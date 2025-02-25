using Microsoft.AspNetCore.Mvc;
using SolarflowSource.Server.Models;
using System.Collections;

namespace SolarflowSource.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UserController : ControllerBase
{
    // Mock user for validation
    private User mockUser = new User(
        "JohnDoe",
        "johndoe@example.com",
        "12345", 
        false,
        "https://api.solarflow.com"
    );

    private readonly ILogger<UserController> _logger;

    public UserController(ILogger<UserController> logger)
    {
        _logger = logger;
    }

    [HttpPost("authenticate")]
    public IActionResult PostUserAuthentication([FromBody] UserAuthRequest request)
    {
        System.Diagnostics.Debug.WriteLine($"Received authentication request: Username = {request.Username}, Password = {request.Password}");

        _logger.LogInformation("Authentication attempt: Username = {Username}, Password = {Password}", request.Username, request.Password);

        if (mockUser.Username == request.Username && mockUser.Password == request.Password)
        {
            _logger.LogInformation("Authentication successful for user: {Username}", request.Username);
            return Ok(mockUser);
        }
        else
        {
            _logger.LogWarning("Failed authentication attempt for user: {Username}", request.Username);
            return Unauthorized(new { message = "Invalid credentials." });
        }
    }

    [HttpPost("recover-account")]
    public IActionResult RecoverAccount([FromBody] AccountRecoveryRequest request)
    {
        System.Diagnostics.Debug.WriteLine($"Received account recovery request: Username = {request.Username}");

        _logger.LogInformation("Account recovery attempt: Username = {Username}", request.Username);

        if (request.Username == mockUser.Username)
        {
            _logger.LogInformation("Account recovery successful for user: {Username}", request.Username);
            return Ok(new { message = "Account recovery email sent." });
        }
        else
        {
            _logger.LogWarning("Failed account recovery attempt for user: {Username}", request.Username);
            return BadRequest(new { message = "User not found." });
        }
    }

    public class UserAuthRequest
    {
        public string Username { get; set; }
        public string Password { get; set; } 
    }

    public class AccountRecoveryRequest
    {
        public string Username { get; set; }
    }
}
