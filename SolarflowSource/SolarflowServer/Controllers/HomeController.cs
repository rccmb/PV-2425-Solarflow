using System.Text.Json;
using Microsoft.AspNetCore.Mvc;

namespace SolarflowServer.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HomeController : ControllerBase
{

    [HttpGet("latest")]
    public IActionResult GetLatestData()
    {
        // TODO: Get Consumption Data
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
    public IActionResult GetConsumptionData()
    {
        // TODO: Get Consumption Data
        const int days = 8;
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