using SolarflowServer.DTOs.Hub;
using SolarflowServer.Models;
using SolarflowServer.Services.Interfaces;

namespace SolarflowServer.Services;

public class EnergyRecordService(ApplicationDbContext context) : IEnergyRecordService
{
    public async Task<IEnumerable<EnergyRecordDTO>> GetEnergyRecords(int hubId, DateTime? startDate,
        DateTime? endDate, EnergySource? source, EnergySource? target)
    {
        var query = context.EnergyRecords.AsQueryable();

        if (hubId > 0) query = query.Where(er => er.HubId == hubId);
        if (startDate.HasValue) query = query.Where(er => er.Timestamp >= startDate.Value);
        if (endDate.HasValue) query = query.Where(er => er.Timestamp <= endDate.Value);
        if (source.HasValue) query = query.Where(er => er.Source == source.Value);
        if (target.HasValue) query = query.Where(er => er.Target == target.Value);

        return await query.Select(er => new EnergyRecordDTO
        {
            HubID = er.HubId,
            Value = er.Value,
            Source = er.Source,
            Target = er.Target,
            Timestamp = er.Timestamp
        }).ToListAsync();
    }

    public async Task<IEnumerable<EnergyRecordDTO>> AddEnergyRecords(object data)
    {
        var energyRecords = new List<EnergyRecordDTO>();

        switch (data)
        {
            case List<EnergyRecordDTO> recordList:
            {
                foreach (var record in recordList)
                {
                    var energyRecordDto = await _AddEnergyRecord(record);
                    energyRecords.Add(energyRecordDto);
                }

                break;
            }
            case EnergyRecordDTO record:
            {
                var energyRecordDto = await _AddEnergyRecord(record);
                energyRecords.Add(energyRecordDto);
                break;
            }
        }

        return energyRecords;
    }

    // Helper method to create a single energy record
    private async Task<EnergyRecordDTO> _AddEnergyRecord(EnergyRecordDTO data)
    {
        var hub = await context.Hubs.FirstOrDefaultAsync(h => h.Id == data.HubID);

        if (hub == null) throw new Exception("Hub not found.");

        var energyRecord = new EnergyRecord
        {
            HubId = hub.Id,
            Value = data.Value,
            Source = data.Source,
            Target = data.Target,
            Timestamp = data.Timestamp ?? DateTime.UtcNow
        };

        context.EnergyRecords.Add(energyRecord);
        await context.SaveChangesAsync();

        return new EnergyRecordDTO
        {
            HubID = energyRecord.HubId,
            Value = energyRecord.Value,
            Source = energyRecord.Source,
            Target = energyRecord.Target,
            Timestamp = energyRecord.Timestamp
        };
    }
}