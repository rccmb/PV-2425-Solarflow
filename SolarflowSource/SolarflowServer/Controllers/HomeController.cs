using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SolarflowServer.Services.Interfaces;

namespace SolarflowServer.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class HomeController(
    ApplicationDbContext context,
    IEnergyRecordService energyRecordService,
    UserManager<ApplicationUser> userManager) : Controller
{
    private readonly ApplicationDbContext _context = context;
    private readonly IEnergyRecordService _energyRecordService = energyRecordService;
    private readonly UserManager<ApplicationUser> _userManager = userManager;


    [HttpGet("latest")]
    public IActionResult GetLatestData()
    {
        // TODO: Get House Data
        const int days = 1;
        var random = new Random();
        var startDate = DateTime.Today;
        var data = Enumerable.Range(0, days)
            .Select(i => new
            {
                date = startDate.AddDays(i).ToString("dd/MM"),
                consumption = random.Next(-100, 0),
                gain = random.Next(0, 50)
            })
            .ToArray();

        var jsonString = JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true });

        return Content(jsonString, "application/json");
    }

    [HttpGet("consumption")]
    public async Task<IActionResult> GetConsumptionData(int? hubId, DateTime? startDate, DateTime? endDate)
    {
        // Fetch the user
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
            return Unauthorized(); // Return unauthorized if user is not found


        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null)
            return Unauthorized(new { error = "User not authenticated." });
        Console.WriteLine(userId);

        // Fetch data from the service
        var data = await _energyRecordService.GetEnergyRecords(1, hubId, startDate, endDate);
        return Json(data);
    }

    [HttpGet("prevision")]
    public async Task<IActionResult> GetPrevisionData()
    {
        // TODO: Get real data
        const int days = 8;
        var startDate = DateTime.Today;
        var random = new Random();
        string[] weatherOptions = { "Clear", "Partly Cloudy", "Cloudy", "Very Cloudy" };
        var data = Enumerable.Range(0, days)
            .Select(i => new
            {
                forecastDate = startDate.AddDays(i).ToString("dd/MM"),
                solarHoursExpected = random.Next(0, 50),
                weatherCondition = weatherOptions[random.Next(weatherOptions.Length)] // Randomly select weather
            })
            .ToArray();

        var jsonString = JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true });

        return Content(jsonString, "application/json");
    }
}