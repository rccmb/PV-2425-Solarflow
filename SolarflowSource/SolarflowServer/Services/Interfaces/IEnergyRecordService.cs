using SolarflowServer.DTOs.Hub;
using SolarflowServer.Models;

namespace SolarflowServer.Services.Interfaces;
using System.Linq;
public interface IEnergyRecordService
{
    Task<IEnumerable<EnergyRecordDTO>> AddEnergyRecords(object data);

    Task<IEnumerable<EnergyRecordDTO>> GetEnergyRecords(int userId, int? hubId, DateTime? startDate,
        DateTime? endDate);
}