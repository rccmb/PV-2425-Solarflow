using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Data;
using Microsoft.Data.SqlClient;
using Dapper;

[ApiController]
[Route("api/[controller]")]
public class UserController : ControllerBase
{
    private readonly ILogger<UserController> _logger;
    private readonly string _connectionString;

    public UserController(ILogger<UserController> logger, IConfiguration configuration)
    {
        _logger = logger;
        _connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new ArgumentNullException(nameof(configuration));
    }

    [HttpPost("authenticate")]
    public IActionResult PostUserAuthentication([FromBody] UserAuthRequest request)
    {
        _logger.LogInformation("Authentication attempt: Username = {Username}", request.Email);

        using (var connection = new SqlConnection(_connectionString))
        {
            var parameters = new DynamicParameters();
            parameters.Add("@Identifier", request.Email, DbType.String);
            parameters.Add("@Password", request.Password, DbType.String);
            parameters.Add("@AuthResult", dbType: DbType.Int32, direction: ParameterDirection.Output);

            connection.Execute("sp_AuthenticateAccount", parameters, commandType: CommandType.StoredProcedure);

            int authResult = parameters.Get<int>("@AuthResult");

            if (authResult == 1)
            {
                _logger.LogInformation("Authentication successful for user: {Username}", request.Email);
                return Ok(new { message = "Authentication successful." });
            }
            else
            {
                _logger.LogWarning("Failed authentication attempt for user: {Username}", request.Email);
                return Unauthorized(new { message = "Invalid credentials." });
            }
        }
    }

    [HttpPost("recover-account")]
    public IActionResult RecoverAccount([FromBody] AccountRecoveryRequest request)
    {
        _logger.LogInformation("Account recovery attempt: Username = {Username}", request.Email);

        using (var connection = new SqlConnection(_connectionString))
        {
            var userExists = connection.ExecuteScalar<int>(
                "SELECT COUNT(1) FROM UserAccount WHERE email = @Email",
                new { Email = request.Email });

            if (userExists == 0)
            {
                _logger.LogWarning("Failed account recovery attempt for user: {Username}", request.Email);
                return BadRequest(new { message = "User not found." });
            }

            // In real implementation, send a password reset email
            _logger.LogInformation("Account recovery successful for user: {Username}", request.Email);
            return Ok(new { message = "Account recovery email sent." });
        }
    }

    public class UserAuthRequest
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public class AccountRecoveryRequest
    {
        public string Email { get; set; } = string.Empty;
    }
}

