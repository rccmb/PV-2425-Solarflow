using SolarflowClient.Models.Enums;

namespace SolarflowClient.Models.ViewModels.Battery;

/// <summary>
/// Represents the data required to retrieve and display battery details.
/// </summary>
public class GetBatteryViewModel
{
    /// <summary>
    /// Gets or sets the maximum capacity of the battery in kilowatt-hours.
    /// </summary>
    public double CapacityMax { get; set; }

    /// <summary>
    /// Gets or sets the charge rate of the battery in kilowatts per hour.
    /// </summary>
    public double ChargeRate { get; set; }

    /// <summary>
    /// Gets or sets the discharge rate of the battery in kilowatts per hour.
    /// </summary>
    public double DischargeRate { get; set; }

    /// <summary>
    /// Gets or sets the current charge level of the battery as a percentage of its maximum capacity.
    /// </summary>
    public int ChargeLevel { get; set; }

    /// <summary>
    /// Gets or sets the source used to charge the battery.
    /// </summary>
    public BatterySource ChargeSource { get; set; }

    /// <summary>
    /// Gets or sets the operational charge mode of the battery.
    /// </summary>
    public BatteryMode ChargeMode { get; set; }

    /// <summary>
    /// Gets or sets the minimal threshold percentage for the battery's charge level.
    /// </summary>
    public int ThresholdMin { get; set; }

    /// <summary>
    /// Gets or sets the maximum threshold percentage for the battery's charge level.
    /// </summary>
    public int ThresholdMax { get; set; }

    /// <summary>
    /// Gets or sets the start time for energy spending (charging grid) as a time-of-day.
    /// </summary>
    public TimeSpan ChargeGridStartTime { get; set; }

    /// <summary>
    /// Gets or sets the end time for energy spending (charging grid) as a time-of-day.
    /// </summary>
    public TimeSpan ChargeGridEndTime { get; set; }

    /// <summary>
    /// Gets or sets the timestamp of the last update to the battery's data.
    /// </summary>
    public DateTime LastUpdate { get; set; }
}