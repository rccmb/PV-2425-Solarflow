using Microsoft.EntityFrameworkCore;
using SolarflowServer.DTOs.Hub;
using SolarflowServer.Models;
using SolarflowServer.Services.Interfaces;

namespace SolarflowServer.Services;

public class EnergyRecordService(ApplicationDbContext context) : IEnergyRecordService
{
    /// <summary>
    ///     Adds energy records to the database. Supports adding a single record or a list of records.
    /// </summary>
    /// <param name="data">The energy record or list of energy records to be added.</param>
    /// <returns>A task that represents the asynchronous operation, containing the added energy records as DTOs.</returns>
    public async Task<IEnumerable<EnergyRecordDTO>> AddEnergyRecords(object data)
    {
        switch (data)
        {
            case List<EnergyRecordDTO> recordList:
            {
                var tasks = recordList.Select(AddEnergyRecord);
                return await Task.WhenAll(tasks);
            }
            case EnergyRecordDTO record:
            {
                var energyRecordDto = await AddEnergyRecord(record);
                return new[] { energyRecordDto };
            }
            default:
                return Enumerable.Empty<EnergyRecordDTO>();
        }
    }

    /// <summary>
    ///     Retrieves energy records based on the provided filters (user ID, hub ID, and date range).
    /// </summary>
    /// <param name="userId">The user ID to filter energy records by.</param>
    /// <param name="startDate">The optional start date for filtering energy records.</param>
    /// <param name="endDate">The optional end date for filtering energy records.</param>
    /// <returns>A task that represents the asynchronous operation, containing a list of energy records matching the filters.</returns>
    public async Task<IEnumerable<EnergyRecordDTO>> GetEnergyRecords(int userId, DateTime? startDate,
        DateTime? endDate)
    {
        var query = context.EnergyRecords
            .Where(r => r.ApplicationUser.Id == userId);

        if (startDate.HasValue) query = query.Where(er => er.Timestamp >= startDate.Value);
        if (endDate.HasValue) query = query.Where(er => er.Timestamp <= endDate.Value);

        return await query.Select(er => MapToDto(er)).ToListAsync();
    }

    /// <summary>
    ///     Adds a single energy record to the database.
    /// </summary>
    /// <param name="data">The energy record DTO to be added.</param>
    /// <returns>A task that represents the asynchronous operation, containing the added energy record as a DTO.</returns>
    private async Task<EnergyRecordDTO> AddEnergyRecord(EnergyRecordDTO data)
    {
        var hub = await context.Users.FirstOrDefaultAsync(h => h.Id == data.ApplicationUserId);
        if (hub == null) throw new Exception("Hub not found.");

        var record = MapFromDto(data);
        context.EnergyRecords.Add(record);
        await context.SaveChangesAsync();

        return MapToDto(record);
    }

    /// <summary>
    ///     Maps an <see cref="EnergyRecordDTO" /> to an <see cref="EnergyRecord" />.
    /// </summary>
    /// <param name="dto">The energy record DTO to be mapped.</param>
    /// <returns>The mapped <see cref="EnergyRecord" />.</returns>
    private static EnergyRecord MapFromDto(EnergyRecordDTO dto)
    {
        return new EnergyRecord
        {
            ApplicationUserId = dto.ApplicationUserId,
            Timestamp = dto.Timestamp,
            House = dto.House,
            Grid = dto.Grid,
            Solar = dto.Solar,
            Battery = dto.Battery
        };
    }

    /// <summary>
    ///     Maps an <see cref="EnergyRecord" /> to an <see cref="EnergyRecordDTO" />.
    /// </summary>
    /// <param name="record">The energy record to be mapped.</param>
    /// <returns>The mapped <see cref="EnergyRecordDTO" />.</returns>
    private static EnergyRecordDTO MapToDto(EnergyRecord record)
    {
        return new EnergyRecordDTO
        {
            ApplicationUserId = record.ApplicationUserId,
            Timestamp = record.Timestamp,
            House = record.House,
            Grid = record.Grid,
            Solar = record.Solar,
            Battery = record.Battery
        };
    }
}