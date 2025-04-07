using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SolarflowServer.DTOs.Hub;
using SolarflowServer.Services.Interfaces;

namespace SolarflowServer.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class HubController(
    IEnergyRecordService energyRecordService,
    UserManager<ApplicationUser> userManager) : ControllerBase
{
    private readonly IEnergyRecordService _energyRecordService = energyRecordService;
    private readonly UserManager<ApplicationUser> _userManager = userManager;


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