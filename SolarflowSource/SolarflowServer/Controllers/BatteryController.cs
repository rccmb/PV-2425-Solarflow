using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SolarflowServer.DTOs;
using SolarflowServer.DTOs.SolarflowServer.DTOs;
using SolarflowServer.Models;
using SolarflowServer.Services;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using SolarflowServer.Services.Interfaces;

/// <summary>
/// Controller for handling battery-related actions for authenticated users.
/// </summary>
[Authorize]
[ApiController]
[Route("api/battery")]
public class BatteryController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly IAuditService _auditService;

    /// <summary>
    /// Initializes a new instance of the <see cref="BatteryController"/> class.
    /// </summary>
    /// <param name="context">The application database context used for battery operations.</param>
    /// <param name="auditService">The audit service used for logging actions.</param>
    public BatteryController(ApplicationDbContext context, IAuditService auditService)
    {
        _context = context;
        _auditService = auditService;
    }

    /// <summary>
    /// Retrieves the battery associated with the currently authenticated user.
    /// </summary>
    /// <returns>The battery details of the authenticated user if found, or an appropriate error message if not authenticated, invalid, or not found.</returns>
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
            .FirstOrDefaultAsync();

        if (battery == null)
        {
            return NotFound(new { error = "Battery not found" });
        }

        return Ok(battery);
    }

    /// <summary>
    /// Updates the battery settings for the currently authenticated user.
    /// </summary>
    /// <param name="model">The battery settings to be updated.</param>
    /// <returns>A result indicating the success or failure of the battery update process.</returns>
    [HttpPost("update-battery")]
    public async Task<IActionResult> UpdateBattery([FromBody] BatteryDTO model)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        if (model.MaximumTreshold < model.MinimalTreshold)
        {
            return BadRequest(new { error = "Maximum Threshold cannot be lower than Minimal Threshold." });
        }

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null)
            return Unauthorized(new { error = "User not authenticated." });

        if (!int.TryParse(userId, out var parsedUserId))
            return BadRequest(new { error = "Invalid user ID" });

        var battery = await _context.Batteries.FirstOrDefaultAsync(b => b.UserId == parsedUserId);
        if (battery == null)
        {
            // await _auditService.LogAsync(userId, "Battery Update", "Failed - Not Found", GetClientIPAddress());
            return NotFound(new { error = "Battery not found" });
        }

        battery.ChargingSource = model.ChargingSource;
        battery.BatteryMode = model.BatteryMode;
        battery.MinimalTreshold = model.MinimalTreshold;
        battery.MaximumTreshold = model.MaximumTreshold;
        battery.SpendingStartTime = model.SpendingStartTime;
        battery.SpendingEndTime = model.SpendingEndTime;
        battery.LastUpdate = DateTime.UtcNow.ToString();

        _context.Batteries.Update(battery);
        await _context.SaveChangesAsync();

        // await _auditService.LogAsync(userId, "Battery Update", "Battery Successfully Updated", GetClientIPAddress());
        return Ok(new { message = "Battery settings updated successfully!" });
    }

    private string GetClientIPAddress()
    {
        if (HttpContext?.Connection?.RemoteIpAddress == null)
        {
            return "127.0.0.1"; // Default for testing
        }
        return HttpContext.Connection.RemoteIpAddress.ToString();
    }
}




