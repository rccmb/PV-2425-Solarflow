﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SolarflowServer.Services;
using System.Security.Claims;


//[Authorize]
[ApiController]
[Route("api/forecast")]
public class ForecastController : ControllerBase
{
    private readonly ForecastService _forecastService;
    private readonly ApplicationDbContext _context;

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
