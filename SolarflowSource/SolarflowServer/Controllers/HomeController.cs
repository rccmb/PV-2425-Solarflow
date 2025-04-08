using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SolarflowServer.Services;
using SolarflowServer.Services.Interfaces;

namespace SolarflowServer.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class HomeController(
    IEnergyRecordService energyRecordService,
    ForecastService forecastService,
    ApplicationDbContext context,
    UserManager<ApplicationUser> userManager) : Controller
{
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