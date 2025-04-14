using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SolarflowClient.Models.Enums;
using SolarflowServer.Services;
using SolarflowServer.Services.Interfaces;

namespace SolarflowServer.Controllers;

/// <summary>
///     Controller for managing energy records and forecast data related to the user's hubs.
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
    ///     Retrieves energy records data for the user's hubs within a specified date range.
    /// </summary>
    /// <param name="hubId">The ID of the hub to fetch data for (optional).</param>
    /// <param name="startDate">The start date for the data range (optional).</param>
    /// <param name="endDate">The end date for the data range (optional).</param>
    /// <param name="timeInterval">What time measure to group the data</param>
    /// <returns>A JSON response with the energy records data.</returns>
    [HttpGet("records")]
    public async Task<IActionResult> GetRecords(DateTime? startDate, DateTime? endDate,
        TimeInterval timeInterval)
    {
        // Fetch the user
        var user = await userManager.GetUserAsync(User);
        if (user == null)
            return Unauthorized(); // Return unauthorized if user is not found


        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null)
            return Unauthorized(new { error = "User not authenticated." });

        // Fetch data from the service
        var data = await energyRecordService.GetEnergyRecords(user.Id, startDate, endDate, timeInterval);
        return Json(data);
    }

    [HttpGet("last")]
    public async Task<IActionResult> GetLastRecord()
    {
        // Fetch the user
        var user = await userManager.GetUserAsync(User);
        if (user == null)
            return Unauthorized(); // Return unauthorized if user is not found


        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null)
            return Unauthorized(new { error = "User not authenticated." });

        // Fetch data from the service
        var data = await energyRecordService.GetLastEnergyRecord(user.Id);
        return Json(data);
    }


    /// <summary>
    ///     Retrieves the forecasted data for the user's first hub.
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

        var data = await forecastService.GetForecast(user.Latitude, user.Longitude);
        Console.WriteLine(data);
        return Json(data);
    }
}