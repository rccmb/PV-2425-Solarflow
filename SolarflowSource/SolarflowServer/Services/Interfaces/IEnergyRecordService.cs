using SolarflowServer.DTOs.Hub;

namespace SolarflowServer.Services.Interfaces;

public interface IEnergyRecordService
{
    Task<IEnumerable<EnergyRecordDTO>> AddEnergyRecords(object data);

    Task<IEnumerable<EnergyRecordDTO>> GetEnergyRecords(int userId, int? hubId, DateTime? startDate,
        DateTime? endDate);
}