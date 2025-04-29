using SolarflowClient.Models.Enums;
using SolarflowServer.DTOs.Hub;

namespace SolarflowServer.Services.Interfaces;

/// <summary>
/// Defines the contract for managing energy records, including adding, retrieving, and processing records.
/// </summary>
public interface IEnergyRecordService
{
    /// <summary>
    /// Adds energy records to the database. Supports adding a single record or a list of records.
    /// </summary>
    /// <param name="data">The energy record or list of energy records to be added.</param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains the added energy records as a collection of <see cref="EnergyRecordDTO"/> objects.
    /// </returns>
    Task<IEnumerable<EnergyRecordDTO>> AddEnergyRecords(object data);

    /// <summary>
    /// Retrieves aggregated energy records based on the provided filters and grouping interval.
    /// </summary>
    /// <param name="userId">The unique identifier of the user whose energy records are being retrieved.</param>
    /// <param name="startDate">The optional start date for filtering the records.</param>
    /// <param name="endDate">The optional end date for filtering the records.</param>
    /// <param name="timeInterval">The time interval for grouping the records.</param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains a collection of <see cref="EnergyRecordDTO"/> objects.
    /// </returns>
    Task<IEnumerable<EnergyRecordDTO>> GetEnergyRecords(int userId, DateTime? startDate,
        DateTime? endDate, TimeInterval? timeInterval);

    /// <summary>
    /// Retrieves the most recent energy record for the specified user.
    /// </summary>
    /// <param name="userId">The unique identifier of the user whose most recent energy record is being retrieved.</param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains the most recent <see cref="EnergyRecordDTO"/> for the specified user, or <c>null</c> if no record exists.
    /// </returns>
    Task<EnergyRecordDTO?> GetLastEnergyRecord(int userId);
}