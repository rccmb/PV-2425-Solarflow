using SolarflowClient.Models.Enums;
using SolarflowServer.DTOs.Hub;

namespace SolarflowServer.Services.Interfaces;

public interface IEnergyRecordService
{
    Task<IEnumerable<EnergyRecordDTO>> AddEnergyRecords(object data);

    Task<IEnumerable<EnergyRecordDTO>> GetEnergyRecords(int userId, DateTime? startDate,
        DateTime? endDate, TimeInterval? timeInterval);


    Task<EnergyRecordDTO?> GetLastEnergyRecord(int userId);
}