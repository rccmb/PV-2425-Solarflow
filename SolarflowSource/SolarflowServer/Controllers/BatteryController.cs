using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SolarflowServer.DTOs.SolarflowServer.DTOs;
using SolarflowServer.Services;

[Authorize]
[ApiController]
[Route("api/battery")]
public class BatteryController(ApplicationDbContext context, IAuditService auditService) : ControllerBase
{
    [HttpGet("get-battery")]
    public async Task<IActionResult> GetBattery()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null)
            return Unauthorized(new { error = "User not authenticated." });

        if (!int.TryParse(userId, out var parsedUserId))
            return BadRequest(new { error = "Invalid user ID" });

        var battery = await context.Batteries
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
        {
            await auditService.LogAsync(userId, "Battery Access", "Failed - Not Found", GetClientIPAddress());
            return NotFound(new { error = "Battery not found" });
        }

        await auditService.LogAsync(userId, "Battery Access", "Battery Data Retrieved", GetClientIPAddress());
        return Ok(battery);
    }

    [HttpPost("update-battery")]
    public async Task<IActionResult> UpdateBattery([FromBody] BatteryDTO model)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        if (model.MaximumTreshold < model.MinimalTreshold)
            return BadRequest(new { error = "Maximum Threshold cannot be lower than Minimal Threshold." });

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null)
            return Unauthorized(new { error = "User not authenticated." });

        if (!int.TryParse(userId, out var parsedUserId))
            return BadRequest(new { error = "Invalid user ID" });

        var battery = await context.Batteries.FirstOrDefaultAsync(b => b.UserId == parsedUserId);
        if (battery == null)
        {
            await auditService.LogAsync(userId, "Battery Update", "Failed - Not Found", GetClientIPAddress());
            return NotFound(new { error = "Battery not found" });
        }

        battery.ChargingSource = model.ChargingSource;
        battery.BatteryMode = model.BatteryMode;
        battery.MinimalTreshold = model.MinimalTreshold;
        battery.MaximumTreshold = model.MaximumTreshold;
        battery.SpendingStartTime = model.SpendingStartTime;
        battery.SpendingEndTime = model.SpendingEndTime;
        battery.LastUpdate = DateTime.UtcNow.ToString();

        context.Batteries.Update(battery);
        await context.SaveChangesAsync();

        await auditService.LogAsync(userId, "Battery Update", "Battery Successfully Updated", GetClientIPAddress());
        return Ok(new { message = "Battery settings updated successfully!" });
    }

    private string GetClientIPAddress()
    {
        return HttpContext?.Connection?.RemoteIpAddress == null
            ? "127.0.0.1"
            : // Default for testing
            HttpContext.Connection.RemoteIpAddress.ToString();
    }
}