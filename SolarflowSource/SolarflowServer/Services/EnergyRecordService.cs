using Microsoft.EntityFrameworkCore;
using SolarflowServer.DTOs.Hub;
using SolarflowServer.Models;
using SolarflowServer.Services.Interfaces;

namespace SolarflowServer.Services;

public class EnergyRecordService(ApplicationDbContext context) : IEnergyRecordService
{
    public async Task<IEnumerable<EnergyRecordDTO>> GetEnergyRecords(int userId, int? hubId, DateTime? startDate,
        DateTime? endDate)
    {
        var query = context.EnergyRecords
            .Include(r => r.Hub)
            .Where(r => r.Hub.UserId == userId);

        if (hubId is > 0) query = query.Where(er => er.HubId == hubId);
        if (startDate.HasValue) query = query.Where(er => er.Timestamp >= startDate.Value);
        if (endDate.HasValue) query = query.Where(er => er.Timestamp <= endDate.Value);

        return await query.Select(er => MapToDto(er)).ToListAsync();
    }

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

    private async Task<EnergyRecordDTO> AddEnergyRecord(EnergyRecordDTO data)
    {
        var hub = await context.Hubs.FirstOrDefaultAsync(h => h.Id == data.HubId);
        if (hub == null) throw new Exception("Hub not found.");

        var record = MapFromDto(data);
        context.EnergyRecords.Add(record);
        await context.SaveChangesAsync();

        return MapToDto(record);
    }

    private static EnergyRecord MapFromDto(EnergyRecordDTO dto)
    {
        return new EnergyRecord
        {
            HubId = dto.HubId,
            Timestamp = dto.Timestamp,
            House = dto.House,
            Grid = dto.Grid,
            Solar = dto.Solar,
            Battery = dto.Battery
        };
    }

    private static EnergyRecordDTO MapToDto(EnergyRecord record)
    {
        return new EnergyRecordDTO
        {
            HubId = record.HubId,
            Timestamp = record.Timestamp,
            House = record.House,
            Grid = record.Grid,
            Solar = record.Solar,
            Battery = record.Battery
        };
    }
}