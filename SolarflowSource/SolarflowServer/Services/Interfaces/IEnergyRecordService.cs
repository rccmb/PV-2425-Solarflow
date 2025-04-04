using SolarflowServer.DTOs.Hub;

namespace SolarflowServer.Services.Interfaces;

public interface IEnergyRecordService
{
    Task<IEnumerable<EnergyRecordDTO>> AddEnergyRecords(object data);

    Task<IEnumerable<EnergyRecordDTO>> GetEnergyRecords(int hubId, DateTime? startDate, DateTime? endDate,
        EnergySource? source, EnergySource? target);
}