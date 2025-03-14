using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using SolarflowServer.Services;

[Route("api/weather")]
[ApiController]
public class WindyController : ControllerBase
{
    private readonly IWindyService _windyService;

    public WindyController(IWindyService windyService)
    {
        _windyService = windyService;
    }

    [HttpGet("forecast")]
    public async Task<IActionResult> GetWeatherForecast([FromQuery] double lat, [FromQuery] double lon)
    {
        if (lat == 0 || lon == 0)
            return BadRequest();

        JObject weatherData;
        try
        {
            weatherData = await _windyService.GetWeatherDataAsync(lat, lon);
        }
        catch
        {
            return StatusCode(500);
        }

        return Ok(weatherData);
    }
}