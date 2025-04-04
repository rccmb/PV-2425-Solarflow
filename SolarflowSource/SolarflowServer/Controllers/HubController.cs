using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SolarflowServer.DTOs.Hub;
using SolarflowServer.Models;
using SolarflowServer.Services.Interfaces;

namespace SolarflowServer.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class HubController(IEnergyRecordService energyRecordService, IUserHubService userHubService) : ControllerBase
{
    private readonly IEnergyRecordService _energyRecordService = energyRecordService;
    private readonly IUserHubService _userHubService = userHubService;

    [HttpPost]
    [Route("record")]
    public async Task<IActionResult> AddEnergyRecord([FromBody] EnergyRecordDTO data)
    {
        try
        {
            var dto = await _energyRecordService.AddEnergyRecords(data);
            return Ok(dto);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message); // Handle errors (e.g., Hub not found)
        }
    }

    // GET - Retrieve energy records with optional filters (Date, Source, Target, etc.)
    [Authorize]
    [HttpGet]
    [Route("record")]
    public async Task<IActionResult> GetEnergyRecords(
        [FromQuery] int? hubId,
        [FromQuery] DateTime? startDate,
        [FromQuery] DateTime? endDate)
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(userIdString, out var userId))
            return Unauthorized();

        var records = await _energyRecordService.GetEnergyRecords(userId, hubId, startDate, endDate);
        return Ok(records);
    }

    [HttpPost]
    [Route("hub/{hubId}/energy-iteration")]
    public async Task<IActionResult> EnergyIteration(int hubId)
    {
        // Get the current authenticated user's ID from the claims
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(userIdString, out var userId))
            return Unauthorized("User not authenticated.");

        // Verify if the user owns the hub
        var ownsHub = await _userHubService.UserOwnsHubAsync(userId, hubId);
        if (!ownsHub)
            return Unauthorized("User does not own this hub.");


        // Parameters + DTO -------------------------------------------------------------------------------

        var consumption = -5.0f; // GET CONSUMPTION KWH
        var solar = 3.0f; // GET SOLAR KWH
        var grid = 10.0f; // GET GRID KWH

        var dto = new EnergyRecordDTO
        {
            HubId = hubId,
            Timestamp = DateTime.Now,
            Consumption = consumption,
            Grid = 0.0f,
            Solar = solar,
            Battery = 0.0f
        };


        // Quotas -----------------------------------------------------------------------------------------

        var quotaConsumption = Math.Abs(dto.Consumption);
        var quotaSolar = dto.Solar;
        var quotaGrid = grid;

        Battery? battery = null;
        var quotaBatteryCharge = battery?.ChargeLevel ?? 0.0f;
        var quotaBatteryDischarge = battery?.ChargeLevel ?? 0.0f;
        var isBatteryChargeForced = battery?.Equals("true") ?? false;


        // Consumption ------------------------------------------------------------------------------------

        // Consume from solar
        if (quotaConsumption > 0.0f && quotaSolar > 0.0f)
        {
            var usedSolar = Math.Min(quotaConsumption, quotaSolar);
            quotaConsumption -= usedSolar;
            quotaSolar -= usedSolar;
        }

        // Consume from battery
        if (quotaConsumption > 0.0f && quotaBatteryDischarge > 0.0f)
        {
            var usedBattery = Math.Min(quotaConsumption, quotaBatteryDischarge);
            quotaConsumption -= usedBattery;
            dto.Battery += usedBattery;
        }

        // Consume from grid
        if (quotaConsumption > 0.0f)
        {
            var usedGrid = Math.Min(quotaConsumption, quotaGrid);
            quotaConsumption -= usedGrid;
            dto.Grid += usedGrid;
        }

        // "Trip breaker" if consumption > 0
        if (quotaConsumption > 0.0f) throw new Exception();


        // Battery ----------------------------------------------------------------------------------------

        // Charge battery from solar
        if (quotaBatteryCharge > 0.0f && quotaSolar > 0.0f)
        {
            var usedSolar = Math.Min(quotaBatteryCharge, quotaSolar);
            dto.Battery -= usedSolar;
            quotaSolar -= usedSolar;
        }

        // Charge battery from grid if isForced
        if (quotaBatteryCharge > 0.0f && isBatteryChargeForced && quotaGrid > 0.0f)
        {
            var usedGrid = Math.Min(quotaBatteryCharge, quotaGrid);
            dto.Battery -= usedGrid;
            dto.Grid += usedGrid;
        }

        // Grid -------------------------------------------------------------------------------------------

        // Sell remaining solar energy
        if (quotaSolar > 0)
            dto.Grid -= quotaSolar;


        // Update -----------------------------------------------------------------------------------------

        // Update database
        if (dto.Battery != 0.0f) battery.ChargeLevel += (int)dto.Battery;

        var result = await _energyRecordService.AddEnergyRecords(dto);

        return Ok(result);
    }
}