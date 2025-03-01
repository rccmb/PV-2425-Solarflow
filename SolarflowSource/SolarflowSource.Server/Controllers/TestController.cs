using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace SolarflowSource.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TestController : ControllerBase
    {
        private readonly ILogger<TestController> _logger;
        private readonly string _connectionString;

        public TestController(ILogger<TestController> logger, IConfiguration configuration)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _connectionString = configuration.GetConnectionString("DefaultConnection") ?? throw new ArgumentNullException(nameof(configuration));
        }

        [HttpGet("test-db-connection")]
        public IActionResult TestDatabaseConnection()
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    connection.Open();
                    return Ok("Database connection successful!");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Database connection failed: {Message}", ex.Message);
                return StatusCode(500, $"Database connection failed: {ex.Message}");
            }
        }
    }
}
