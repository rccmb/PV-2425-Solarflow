using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SolarflowServer.DTOs.Hub;
using SolarflowServer.Services.Interfaces;

namespace SolarflowServer.Controllers;

/// <summary>
/// Controller for managing energy records related to the user's hubs.
/// </summary>
[Authorize]
[ApiController]
[Route("api/[controller]")]
public class HubController(
    IEnergyRecordService energyRecordService,
    UserManager<ApplicationUser> userManager) : ControllerBase
{
    private readonly IEnergyRecordService _energyRecordService = energyRecordService;
    private readonly UserManager<ApplicationUser> _userManager = userManager;

    /// <summary>
    /// Retrieves energy records for the user's hubs within a specified date range.
    /// </summary>
    /// <param name="hubId">The ID of the hub to fetch records for (optional).</param>
    /// <param name="startDate">The start date for the record range (optional).</param>
    /// <param name="endDate">The end date for the record range (optional).</param>
    /// <returns>A JSON response with the energy records.</returns>
    [HttpGet("record")]
    public async Task<IActionResult> GetEnergyRecords(
        [FromQuery] int? hubId,
        [FromQuery] DateTime? startDate,
        [FromQuery] DateTime? endDate)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
            return Unauthorized();

        startDate ??= DateTime.Today.AddDays(-7);
        endDate ??= DateTime.Today;

        var records = await _energyRecordService.GetEnergyRecords(user.Id, hubId, startDate.Value, endDate.Value);
        return Ok(records);
    }

    /// <summary>
    /// Adds a new energy record for the user's hubs.
    /// </summary>
    /// <param name="data">The energy record data to be added.</param>
    /// <returns>A JSON response indicating the success or failure of the operation.</returns>
    [Authorize]
    [HttpPost("record")]
    public async Task<IActionResult> AddEnergyRecord([FromBody] EnergyRecordDTO data)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
            return Unauthorized();

        try
        {
            var dto = await _energyRecordService.AddEnergyRecords(data);
            return Ok(dto);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }
}