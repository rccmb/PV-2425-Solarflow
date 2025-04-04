using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SolarflowServer.DTOs.Hub;
using SolarflowServer.Models;

namespace SolarflowServer.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class EnergyRecordsController(ApplicationDbContext context) : ControllerBase
{
    // GET - Retrieve a single EnergyRecord by Id (optional, useful for more detailed views)
    [HttpGet("{id}")]
    public async Task<IActionResult> GetEnergyRecordById(int id)
    {
        // Retrieve EnergyRecord by ID
        var energyRecord = await context.EnergyRecords
            .FirstOrDefaultAsync(e => e.Id == id);

        // If EnergyRecord is NULL, return NotFound response
        if (energyRecord == null) return NotFound($"EnergyRecord with ID {id} not found.");

        // Create EnergyRecord DTO
        var dto = new EnergyRecordDTO
        {
            HubID = energyRecord.HubId,
            Value = energyRecord.Value,
            Source = energyRecord.Source,
            Target = energyRecord.Target,
            Timestamp = energyRecord.Timestamp
        };

        return Ok(dto);
    }

    // GET - Get EnergyRecords with optional filters
    [HttpGet]
    public async Task<IActionResult> GetEnergyRecords(
        [FromQuery] int? userId,
        [FromQuery] int? hubId,
        [FromQuery] DateTime? startDate,
        [FromQuery] DateTime? endDate,
        [FromQuery] EnergySource? source,
        [FromQuery] EnergySource? target)

    {
        // Build the query to retrieve EnergyRecords with filters applied
        var query = context.EnergyRecords.AsQueryable();
        if (userId.HasValue) query = query.Where(e => e.Hub.UserID == userId.Value);
        if (hubId.HasValue) query = query.Where(e => e.HubId == hubId.Value);
        if (startDate.HasValue) query = query.Where(e => e.Timestamp >= startDate.Value);
        if (endDate.HasValue) query = query.Where(e => e.Timestamp <= endDate.Value);
        if (source.HasValue) query = query.Where(e => e.Source == source.Value);
        if (target.HasValue) query = query.Where(e => e.Target == target.Value);


        // Execute the query and get the results
        var energyRecords = await query
            .Include(e => e.Hub)
            .ToListAsync();

        // If no records found, return NoContent
        if (!energyRecords.Any()) return NoContent();

        // Map the EnergyRecords to DTOs for response
        var recordsDto = energyRecords.Select(e => new EnergyRecordDTO
        {
            HubID = e.HubId,
            Value = e.Value,
            Source = e.Source,
            Target = e.Target,
            Timestamp = e.Timestamp
        }).ToList();

        return Ok(recordsDto);
    }


    // POST - Add a new EnergyRecord
    [HttpPost]
    public async Task<IActionResult> AddEnergyRecord([FromBody] EnergyRecordDTO data)
    {
        // Validate and retrieve the Hub
        var hub = await context.Hubs
            .FirstOrDefaultAsync(h => h.Id == data.HubID);

        if (hub == null)
            return BadRequest("Hub not found for the provided User.");

        // Create EnergyRecord
        var record = new EnergyRecord
        {
            HubId = hub.Id,
            Value = data.Value,
            Source = data.Source,
            Target = data.Target,
            Timestamp = data.Timestamp ?? DateTime.UtcNow // Use current UTC time if timestamp is null
        };

        // Add EnergyRecord to database
        context.EnergyRecords.Add(record);
        await context.SaveChangesAsync();

        // Create EnergyRecord DTO
        var dto = new EnergyRecordDTO
        {
            HubID = record.HubId,
            Value = record.Value,
            Source = record.Source,
            Target = record.Target,
            Timestamp = record.Timestamp
        };

        return Ok(dto);
    }
}