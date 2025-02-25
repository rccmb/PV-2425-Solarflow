using Microsoft.AspNetCore.Mvc;
using SolarflowSource.Server.Models;
using System.Collections;

namespace SolarflowSource.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UserController : ControllerBase
{
    private readonly ILogger<UserController> _logger;

    public UserController(ILogger<UserController> logger)
    {
        _logger = logger;
    }

    [HttpPost("authenticate")]
    public IActionResult PostUserAuthentication([FromBody] UserAuthRequest request)
    {
        // Print request data to console
        System.Diagnostics.Debug.WriteLine($"Received authentication request: Username = {request.Username}, Password = {request.Password}");

        // Log request data using ILogger (recommended for production)
        _logger.LogInformation("Authentication attempt: Username = {Username}, Password = {Password}", request.Username, request.Password);

        // Mock user for validation
        User mockUser = new User(
            "JohnDoe",
            "johndoe@example.com",
            "12345", // TODO: Use hashed password instead of plain text.
            false,
            "https://api.solarflow.com"
        );

        if (mockUser.Username == request.Username && mockUser.Password == request.Password)
        {
            _logger.LogInformation("Authentication successful for user: {Username}", request.Username);
            return Ok(mockUser);
        }
        else
        {
            _logger.LogWarning("Failed authentication attempt for user: {Username}", request.Username);
            return Unauthorized("Invalid credentials.");
        }
    }

    public class UserAuthRequest
    {
        public string Username { get; set; }
        public string Password { get; set; } 
    }
}
