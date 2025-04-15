using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SolarflowClient.Models;
using System.Security.Claims;

namespace SolarflowClient.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]

    public class WindyController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly ApplicationDbContext _context;

        // Constructor injecting configuration and database context
        public WindyController(IConfiguration configuration, ApplicationDbContext context)
        {
            _configuration = configuration;
            _context = context;
        }

        /// <summary>
        /// Retrieves Windy API configuration for the authenticated user.
        /// </summary>
        /// <returns>The WindyConfig object containing API key and user coordinates.</returns>
        [HttpGet("config")]
        [Authorize] // Ensures that only authenticated users can access this endpoint
        public IActionResult GetWindyConfig()
        {
            var userId = GetUserId(); // Extracts user ID from JWT claims

            // Retrieves the user from the database
            var user = _context.Users.FirstOrDefault(u => u.Id == userId);
            if (user == null)
                return Unauthorized(); // If user not found, return unauthorized

            // Builds the Windy configuration object using user location and stored API key
            var config = new WindyConfig
            {
                ApiKey = _configuration["WindyMapAPI:Key"], // API key from configuration settings
                Lat = user.Latitude, // User's stored latitude
                Lon = user.Longitude // User's stored longitude
            };

            return Ok(config); // Returns the configuration as JSON
        }

        /// <summary>
        /// Helper method to extract the authenticated user's ID from JWT.
        /// </summary>
        /// <returns>The user ID as an integer.</returns>
        private int GetUserId()
        {
            return int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
        }
    }
}
