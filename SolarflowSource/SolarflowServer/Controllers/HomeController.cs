using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SolarflowServer.Services;
using SolarflowServer.Services.Interfaces;

namespace SolarflowServer.Controllers;

/// <summary>
/// Controller for managing energy consumption and forecast data related to the user's hubs.
/// </summary>
[Authorize]
[ApiController]
[Route("api/[controller]")]
public class HomeController(
    IEnergyRecordService energyRecordService,
    ForecastService forecastService,
    ApplicationDbContext context,
    UserManager<ApplicationUser> userManager) : Controller
{
    /// <summary>
    /// Retrieves energy consumption data for the user's hubs within a specified date range.
    /// </summary>
    /// <param name="hubId">The ID of the hub to fetch data for (optional).</param>
    /// <param name="startDate">The start date for the data range (optional).</param>
    /// <param name="endDate">The end date for the data range (optional).</param>
    /// <returns>A JSON response with the energy consumption data.</returns>
    [HttpGet("consumption")]
    public async Task<IActionResult> GetConsumptionData(int? hubId, DateTime? startDate, DateTime? endDate)
    {
        // Fetch the user
        var user = await userManager.GetUserAsync(User);
        if (user == null)
            return Unauthorized(); // Return unauthorized if user is not found


        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null)
            return Unauthorized(new { error = "User not authenticated." });


        Console.WriteLine(startDate);

        // Fetch data from the service
        var data = await energyRecordService.GetEnergyRecords(user.Id, hubId, startDate, endDate);
        return Json(data);
    }

    /// <summary>
    /// Retrieves the forecasted data for the user's first hub.
    /// </summary>
    /// <returns>A JSON response with the forecasted data.</returns>
    [HttpGet("prevision")]
    public async Task<IActionResult> GetPrevisionData()
    {
        var user = await userManager.GetUserAsync(User);
        if (user == null)
            return Unauthorized();

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null)
            return Unauthorized(new { error = "User not authenticated." });

        var firstHub = context.Hubs.First(h => h.UserId == user.Id);
        var data = await forecastService.GetForecast(firstHub.Latitude, firstHub.Longitude);
        Console.WriteLine(data);
        return Json(data);
    }
}