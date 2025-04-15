using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SolarflowServer.Services;



/// <summary>
/// Controller for managing forecast-related operations, such as updating and retrieving forecast data.
/// </summary>
[ApiController]
[Route("api/forecast")]
public class ForecastController : ControllerBase
{
    private readonly ForecastService _forecastService;
    private readonly ApplicationDbContext _context;

    /// <summary>
    /// Initializes a new instance of the <see cref="ForecastController"/> class.
    /// </summary>
    /// <param name="forecastService">The service responsible for forecast operations.</param>
    /// <param name="context">The application database context used for data operations.</param>
    public ForecastController(ForecastService forecastService, ApplicationDbContext context)
    {
        _forecastService = forecastService;
        _context = context;
    }

  
    [HttpPost("update")]
    public async Task<IActionResult> UpdateForecast([FromQuery] double lat, [FromQuery] double lon, [FromQuery] int days)
    {
        //var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        //if (userId == null || !int.TryParse(userId, out var parsedUserId))
        //   return Unauthorized();

        var battery = await _context.Batteries.FirstOrDefaultAsync(b => b.UserId == 1);
        if (battery == null)
            return NotFound();

       

        await _forecastService.SaveForecastAsync(battery.Id, lat, lon, days);
        return Ok(new { message = "Updated" });
    }

    /// <summary>
    /// Retrieves the saved forecast data for the battery associated with the user.
    /// </summary>
    /// <returns>The list of saved forecasts for the associated battery, or an error if no forecasts are found.</returns>
    [HttpGet("get")]
    public async Task<IActionResult> GetSavedForecasts()
    {
         // var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
         // if (userId == null || !int.TryParse(userId, out var parsedUserId))
         //    return Unauthorized();

         var battery = await _context.Batteries.FirstOrDefaultAsync(b => b.UserId == 1);
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

    [HttpGet("current")]
    public async Task<IActionResult> GetCurrentForecast([FromQuery] double lat, [FromQuery] double lon)
    {
        var result = await _forecastService.GetCurrentForecastAsync(lat, lon);
        if (result == null)
            return NotFound(new { error = "No forecast data found for current time slot." });

        return Ok(result);
    }

}
