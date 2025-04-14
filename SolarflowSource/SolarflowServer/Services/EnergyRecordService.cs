using Microsoft.EntityFrameworkCore;
using SolarflowClient.Models.Enums;
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
    ///     Retrieves aggregated energy records based on the provided filters and grouping interval.
    /// </summary>
    /// <param name="userId">ID of the user.</param>
    /// <param name="startDate">Optional start date filter.</param>
    /// <param name="endDate">Optional end date filter.</param>
    /// <param name="interval">TimeInterval for grouping.</param>
    /// <returns>A task representing the asynchronous operation; the task result contains a list of EnergyRecordDTOs.</returns>
    public async Task<IEnumerable<EnergyRecordDTO>> GetEnergyRecords(
        int userId, DateTime? startDate, DateTime? endDate, TimeInterval? interval)
    {
        // Build the query filtering by user and dates.
        var query = context.EnergyRecords.Where(r => r.ApplicationUser.Id == userId);
        if (startDate.HasValue)
            query = query.Where(er => er.Timestamp >= startDate.Value);
        if (endDate.HasValue)
            query = query.Where(er => er.Timestamp <= endDate.Value);

        // Retrieve raw records.
        var rawRecords = await query.ToListAsync();

        // Aggregate records using our private aggregation method.
        var aggregatedRecords = AggregateRecords(rawRecords, interval);

        // Map the aggregated records to DTOs.
        return aggregatedRecords.Select(MapToDto).ToList();
    }

    /// <summary>
    ///     Aggregates energy records based on the provided time interval.
    ///     For Minute and Hour intervals, averages are computed.
    ///     For larger intervals (Day, Week, Month, Year), the hourly averages are summed.
    /// </summary>
    {
        var query = context.EnergyRecords
            .Where(r => r.ApplicationUser.Id == userId);

        if (startDate.HasValue) query = query.Where(er => er.Timestamp >= startDate.Value);
        if (endDate.HasValue) query = query.Where(er => er.Timestamp <= endDate.Value);
        // If grouping by Minute, group directly by minute and average.
        if (interval == TimeInterval.Minute)
            return records
                .GroupBy(r => new DateTime(r.Timestamp.Year, r.Timestamp.Month, r.Timestamp.Day, r.Timestamp.Hour,
                    r.Timestamp.Minute, 0))
                .Select(g => new EnergyRecord
                {
                    Timestamp = g.Key,
                    House = g.Average(x => x.House),
                    Grid = g.Average(x => x.Grid),
                    Solar = g.Average(x => x.Solar),
                    Battery = g.Average(x => x.Battery)
                })
                .ToList();

        // For Hour or larger intervals, first compute hourly averages.
        var hourlyAverages = records
            .GroupBy(r => new { r.Timestamp.Year, r.Timestamp.Month, r.Timestamp.Day, r.Timestamp.Hour })
            .Select(g => new EnergyRecord
            {
                Timestamp = new DateTime(g.Key.Year, g.Key.Month, g.Key.Day, g.Key.Hour, 0, 0),
                House = g.Average(x => x.House),
                Grid = g.Average(x => x.Grid),
                Solar = g.Average(x => x.Solar),
                Battery = g.Average(x => x.Battery)
            })
            .ToList();

        return interval switch
        {
            // If grouping by Hour, return the hourly averages directly.
            TimeInterval.Hour => hourlyAverages,
            // For larger intervals (Day, Week, Month, Year), sum the hourly averages.
            TimeInterval.Day => hourlyAverages.GroupBy(r => r.Timestamp.Date)
                .Select(g => new EnergyRecord
                {
                    Timestamp = g.Key,
                    House = g.Sum(x => x.House),
                    Grid = g.Sum(x => x.Grid),
                    Solar = g.Sum(x => x.Solar),
                    Battery = g.Sum(x => x.Battery)
                })
                .ToList(),
            TimeInterval.Week => hourlyAverages.GroupBy(r =>
                {
                    // Calculate the Monday for each week.
                    var diff = ((int)r.Timestamp.DayOfWeek - (int)DayOfWeek.Monday + 7) % 7;
                    return r.Timestamp.Date.AddDays(-diff);
                })
                .Select(g => new EnergyRecord
                {
                    Timestamp = g.Key,
                    House = g.Sum(x => x.House),
                    Grid = g.Sum(x => x.Grid),
                    Solar = g.Sum(x => x.Solar),
                    Battery = g.Sum(x => x.Battery)
                })
                .ToList(),
            TimeInterval.Month => hourlyAverages.GroupBy(r => new { r.Timestamp.Year, r.Timestamp.Month })
                .Select(g => new EnergyRecord
                {
                    Timestamp = new DateTime(g.Key.Year, g.Key.Month, 1),
                    House = g.Sum(x => x.House),
                    Grid = g.Sum(x => x.Grid),
                    Solar = g.Sum(x => x.Solar),
                    Battery = g.Sum(x => x.Battery)
                })
                .ToList(),
            TimeInterval.Year => hourlyAverages.GroupBy(r => r.Timestamp.Year)
                .Select(g => new EnergyRecord
                {
                    Timestamp = new DateTime(g.Key, 1, 1),
                    House = g.Sum(x => x.House),
                    Grid = g.Sum(x => x.Grid),
                    Solar = g.Sum(x => x.Solar),
                    Battery = g.Sum(x => x.Battery)
                })
                .ToList(),
            _ => throw new ArgumentOutOfRangeException(nameof(interval), interval, "Unsupported time interval.")
        };
    }


    public async Task<EnergyRecordDTO?> GetLastEnergyRecord(int userId)
    {
        // Retrieve the most recent energy record for the specified user.
        var record = await context.EnergyRecords
            .Where(r => r.ApplicationUser.Id == userId)
            .OrderByDescending(r => r.Timestamp)
            .FirstOrDefaultAsync();

        // If no record exists, return null.
        return record == null
            ? null
            :
            // Map the entity to DTO before returning.
            MapToDto(record);
    }

    /// <summary>
    ///     Adds a single energy record to the database.
    /// </summary>
    /// <param name="data">The energy record DTO to be added.</param>
    /// <returns>A task that represents the asynchronous operation, containing the added energy record as a DTO.</returns>
    private async Task<EnergyRecordDTO?> AddEnergyRecord(EnergyRecordDTO data)
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
    private static EnergyRecordDTO? MapToDto(EnergyRecord record)
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