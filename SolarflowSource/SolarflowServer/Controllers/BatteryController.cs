using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SolarflowServer.DTOs;
using SolarflowServer.DTOs.SolarflowServer.DTOs;
using SolarflowServer.Models;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

[Authorize]
[ApiController]
[Route("api/battery")]
public class BatteryController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public BatteryController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet("get-battery")]
    public async Task<IActionResult> GetBattery()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null)
            return Unauthorized(new { error = "User not authenticated." });

        if (!int.TryParse(userId, out var parsedUserId))
            return BadRequest(new { error = "Invalid user ID" });

        var battery = await _context.Batteries
            .Where(b => b.UserId == parsedUserId)
            .Select(b => new BatteryDTO
            {
                ChargingSource = b.ChargingSource,
                BatteryMode = b.BatteryMode,
                MinimalTreshold = b.MinimalTreshold,
                MaximumTreshold = b.MaximumTreshold,
                SpendingStartTime = b.SpendingStartTime,
                SpendingEndTime = b.SpendingEndTime
            })
            .FirstOrDefaultAsync();

        if (battery == null)
            return NotFound(new { error = "Battery not found" });

        return Ok(battery);
    }

    [HttpPost("update-battery")]
    public async Task<IActionResult> UpdateBattery([FromBody] BatteryDTO model)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null)
            return Unauthorized(new { error = "User not authenticated." });

        if (!int.TryParse(userId, out var parsedUserId))
            return BadRequest(new { error = "Invalid user ID" });

        var battery = await _context.Batteries.FirstOrDefaultAsync(b => b.UserId == parsedUserId);
        if (battery == null)
            return NotFound(new { error = "Battery not found" });

        battery.ChargingSource = model.ChargingSource;
        battery.BatteryMode = model.BatteryMode;
        battery.MinimalTreshold = model.MinimalTreshold;
        battery.MaximumTreshold = model.MaximumTreshold;
        battery.SpendingStartTime = model.SpendingStartTime;
        battery.SpendingEndTime = model.SpendingEndTime;
        battery.LastUpdate = DateTime.UtcNow.ToString();

        _context.Batteries.Update(battery);
        await _context.SaveChangesAsync();

        return Ok(new { message = "Battery settings updated successfully!" });
    }

}




