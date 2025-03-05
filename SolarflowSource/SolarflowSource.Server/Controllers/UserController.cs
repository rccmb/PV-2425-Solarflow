using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Data;
using Microsoft.Data.SqlClient;
using Dapper;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using SolarflowSource.Server.Models;
using System.Text.RegularExpressions;

[ApiController]
[Route("api/[controller]")]
public class UserController : ControllerBase
{
    private readonly ILogger<UserController> _logger;
    private readonly string _connectionString;
    private readonly IConfiguration _configuration;

    public UserController(ILogger<UserController> logger, IConfiguration configuration)
    {
        _logger = logger;
        _connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new ArgumentNullException(nameof(configuration));
        _configuration = configuration;
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
                var token = GenerateJWTToken(request.Email);
                _logger.LogInformation("Authentication successful for user: {Username}", request.Email);
                return Ok(new { token });
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

    [Authorize]
    [HttpGet("profile")]
    public IActionResult GetUserProfile()
    {
        var identity = HttpContext.User.Identity as ClaimsIdentity;

        if (identity == null || !identity.IsAuthenticated)
        {
            return Unauthorized(new { message = "User is not authenticated." });
        }

        var email = identity.FindFirst(ClaimTypes.Email)?.Value;

        Console.WriteLine($"Extracted email: {email}"); // Debugging

        if (string.IsNullOrEmpty(email))
        {
            return Unauthorized(new { message = "Unauthorized access." });
        }

        using (var connection = new SqlConnection(_connectionString))
        {
            var user = connection.QuerySingleOrDefault<User>(
                "sp_GetUserProfile",
                new { Email = email },
                commandType: CommandType.StoredProcedure);

            if (user == null)
            {
                return NotFound(new { message = "User not found." });
            }

            return Ok(user);
        }
    }

    [HttpPost("register")]
    public IActionResult PostUserRegistration([FromBody] AccountRegistrationRequest request)
    {
        _logger.LogInformation("User registration attempt: Email = {Email}", request.Email);

        var errors = new Dictionary<string, string>();
        bool isValid = true;

        // Validate the name.
        if (string.IsNullOrWhiteSpace(request.Name) || request.Name.Length < 3 || request.Name.StartsWith(" "))
        {
            errors["name"] = "Name must have at least 3 characters and must not start with white space.";
            isValid = false;
        }

        // Validate the email.
        var emailPattern = new Regex("^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\\.[a-zA-Z]{2,4}$");
        if (!emailPattern.IsMatch(request.Email))
        {
            errors["email"] = "Invalid email format.";
            isValid = false;
        }

        // Validate the password.
        var passwordPattern = new Regex("^(?!.*\\s).{8,}$");
        if (!passwordPattern.IsMatch(request.Password))
        {
            errors["password"] = "Password must have at least 8 characters with no white spaces.";
            isValid = false;
        }

        if (!isValid)
        {
            return BadRequest(new { errors });
        }

        try
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                var parameters = new DynamicParameters();
                parameters.Add("@Name", request.Name, DbType.String);
                parameters.Add("@Email", request.Email, DbType.String);
                parameters.Add("@Password", request.Password, DbType.String);
                parameters.Add("@Photo", null, DbType.String);
                parameters.Add("@BatteryAPI", null, DbType.String);

                connection.Execute("sp_AddUserAccount", parameters, commandType: CommandType.StoredProcedure);

                _logger.LogInformation("User successfully registered: {Email}", request.Email);
                return Ok(new { message = "User registered successfully. Verification email sent!" });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during user registration: {Email}", request.Email);
            return StatusCode(500, new { message = "Failed to register the user. Try again later." });
        }
    }


    private string GenerateJWTToken(string email)
    {
        var jwtSettings = _configuration.GetSection("JwtSettings");
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["SecretKey"]));

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Email, email),
            new Claim(ClaimTypes.Role, "User")
        };

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMinutes(int.Parse(jwtSettings["ExpirationMinutes"])),
            SigningCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256),
            Issuer = jwtSettings["Issuer"],
            Audience = jwtSettings["Audience"]
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
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

    public class AccountRegistrationRequest
    {
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}

