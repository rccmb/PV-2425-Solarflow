using Microsoft.AspNetCore.Mvc;
using SolarflowServer.Services;
using System;
using System.Threading.Tasks;

namespace SolarflowServer.Controllers
{
    [Route("api/windy")]
    [ApiController]
    public class WindyController : ControllerBase
    {
        private readonly WindyService _windyService;

        public WindyController(WindyService windyService)
        {
            _windyService = windyService;
        }

        [HttpGet("forecast")]
        public async Task<IActionResult> GetWeather(
            [FromQuery] double lat, 
            [FromQuery] double lon,
            [FromQuery] int days)
        {
            try
            {
                var forecast = await _windyService.GetWeatherForecastAsync(lat, lon, days);
                return Ok(forecast);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpGet("solar-forecast")]
        public async Task<IActionResult> GetSolarForecast(
            [FromQuery] double lat, 
            [FromQuery] double lon,
            [FromQuery] int days)
        {
            try
            {
                var result = await _windyService.GetSolarForecastAsync(lat, lon, days);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }
    }
}