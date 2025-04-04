using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SolarflowServer.DTOs.Hub;
using SolarflowServer.Services.Interfaces;

namespace SolarflowServer.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class HubController(IEnergyRecordService energyRecordService) : ControllerBase
{
    private readonly IEnergyRecordService _energyRecordService = energyRecordService;

    [HttpPost]
    [Route("record")]
    public async Task<IActionResult> AddEnergyRecord([FromBody] EnergyRecordDTO data)
    {
        try
        {
            // Call the EnergyRecordService to create the EnergyRecord
            var recordDto = await _energyRecordService.CreateEnergyRecordAsync(data);

            return Ok(recordDto); // Return the created EnergyRecord DTO as a response
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message); // Handle errors (e.g., Hub not found)
        }
    }

    // GET - Retrieve energy records with optional filters (Date, Source, Target, etc.)
    [HttpGet]
    public async Task<IActionResult> GetEnergyRecords(
        [FromQuery] int hubId,
        [FromQuery] DateTime? startDate,
        [FromQuery] DateTime? endDate,
        [FromQuery] EnergySource? source,
        [FromQuery] EnergySource? target)
    {
        var records = await _energyRecordService.GetEnergyRecordsAsync(hubId, startDate, endDate, source, target);
        return Ok(records);
    }
}

}