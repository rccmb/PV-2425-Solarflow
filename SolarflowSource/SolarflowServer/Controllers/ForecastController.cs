using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SolarflowServer.Services;
using System.Security.Claims;

/// <summary>
/// Controller responsible for managing forecast-related operations,
/// such as updating and retrieving weather forecast data linked to the authenticated user.
/// </summary>
[ApiController]
[Route("api/forecast")]
[Authorize]
public class ForecastController : ControllerBase
{
    private readonly ForecastService _forecastService;
    private readonly ApplicationDbContext _context;

    /// <summary>
    /// Initializes a new instance of the <see cref="ForecastController"/> class.
    /// </summary>
    /// <param name="forecastService">The service responsible for forecast operations.</param>
    /// <param name="context">The application database context used for data access.</param>
    public ForecastController(ForecastService forecastService, ApplicationDbContext context)
    {
        _forecastService = forecastService;
        _context = context;
    }

    /// <summary>
    /// Updates the weather forecast for the user's location for the specified number of days.
    /// </summary>
    /// <param name="days">The number of days to include in the forecast update.</param>
    /// <returns>Returns an OK result if the forecast was successfully updated, or an appropriate error.</returns>
    [HttpPost("update")]
    public async Task<IActionResult> UpdateForecast([FromQuery] int days)
    {
        var userId = GetUserId();

        var user = _context.Users.FirstOrDefault(u => u.Id == userId);
        if (user == null)
            return Unauthorized(); // User not found

        var battery = await _context.Batteries.FirstOrDefaultAsync(b => b.UserId == userId);
        if (battery == null)
            return NotFound(); // Battery not associated with user

        await _forecastService.SaveForecastAsync(battery.Id, user.Latitude, user.Longitude, days);
        return Ok(new { message = "Updated" });
    }

    /// <summary>
    /// Retrieves the saved weather forecasts for the battery associated with the authenticated user.
    /// </summary>
    /// <returns>A list of saved forecasts, or an error if none are found.</returns>
    [HttpGet("get")]
    public async Task<IActionResult> GetSavedForecasts()
    {
        var userId = GetUserId();

        var battery = await _context.Batteries.FirstOrDefaultAsync(b => b.UserId == userId);
        if (battery == null)
            return NotFound();

        var forecasts = await _context.Forecasts
            .Where(f => f.BatteryID == battery.Id)
            .OrderBy(f => f.ForecastDate)
            .ToListAsync();

        if (!forecasts.Any())
            return NotFound(new { error = "No forecasts found for this battery." });

        return Ok(forecasts);
    }

    /// <summary>
    /// Retrieves the current weather forecast based on the user's location.
    /// </summary>
    /// <returns>The current forecast data, or an error if unavailable.</returns>
    [HttpGet("current")]
    public async Task<IActionResult> GetCurrentForecast()
    {
        var userId = GetUserId();

        var user = _context.Users.FirstOrDefault(u => u.Id == userId);
        if (user == null)
            return Unauthorized();

        var result = await _forecastService.GetCurrentForecastAsync(user.Latitude, user.Longitude);
        if (result == null)
            return NotFound(new { error = "No forecast data found for current time slot." });

        return Ok(result);
    }

    /// <summary>
    /// Helper method that extracts the authenticated user's ID from the JWT.
    /// </summary>
    /// <returns>The user's ID as an integer.</returns>
    /// <exception cref="UnauthorizedAccessException">Thrown when the user ID is not found or is invalid.</exception>
    private int GetUserId()
    {
        var idClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (idClaim == null || !int.TryParse(idClaim, out var userId))
            throw new UnauthorizedAccessException("Error");
        return userId;
    }
}
