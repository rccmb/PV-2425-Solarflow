using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SolarflowClient.Models;
using System.Security.Claims;

namespace SolarflowClient.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class WindyController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly ApplicationDbContext _context;

        public WindyController(IConfiguration configuration, ApplicationDbContext context)
        {
            _configuration = configuration;
            _context = context;
        }

        [HttpGet("config")]
        [Authorize]
        public IActionResult GetWindyConfig()
        {
            var userId = GetUserId();

            var user = _context.Users.FirstOrDefault(u => u.Id == userId);
            if (user == null)
                return Unauthorized();

            var config = new WindyConfig
            {
                ApiKey = _configuration["WindyMapAPI:Key"],
               
                Lat = user.Latitude,
                Lon = user.Longitude
            };
            Console.WriteLine("000000000000000000");
            Console.WriteLine(config);
            Console.WriteLine("aaaaaaaaaaaaaaaaaaa");
            Console.WriteLine(userId);
            Console.WriteLine("bbbbbbbbbbbbbbbbbb");
            Console.WriteLine(user.Longitude);
            Console.WriteLine("ccccccccccccccccccc");
            Console.WriteLine(user.Latitude);
            Console.WriteLine("ddddddddddddddddddddd");

            return Ok(config);
        }

        private int GetUserId()
        {
            return int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
        }
    }
}