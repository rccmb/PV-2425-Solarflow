using SolarflowServer.Models.Enums;

/// <summary>
/// Represents the data transfer object for battery configuration and status.
/// </summary>
public class BatteryDTO
{
    /// <summary>
    /// Gets or sets the maximum capacity of the battery in kilowatt-hours (kWh).
    /// </summary>
    public double CapacityMax { get; set; }

    /// <summary>
    /// Gets or sets the rate at which the battery can be charged in kilowatts (kW).
    /// </summary>
    public double ChargeRate { get; set; }

    /// <summary>
    /// Gets or sets the rate at which the battery can be discharged in kilowatts (kW).
    /// </summary>
    public double DischargeRate { get; set; }

    /// <summary>
    /// Gets or sets the source used to charge the battery.
    /// </summary>
    /// <remarks>
    /// Possible values are defined in the <see cref="BatterySource"/> enum.
    /// </remarks>
    public BatterySource ChargeSource { get; set; }

    /// <summary>
    /// Gets or sets the operational mode of the battery.
    /// </summary>
    /// <remarks>
    /// Possible values are defined in the <see cref="BatteryMode"/> enum.
    /// </remarks>
    public BatteryMode ChargeMode { get; set; }

    /// <summary>
    /// Gets or sets the minimum threshold percentage for the battery's charge level.
    /// </summary>
    /// <remarks>
    /// This value is used to prevent the battery from discharging below a certain level.
    /// </remarks>
    public int ThresholdMin { get; set; }

    /// <summary>
    /// Gets or sets the maximum threshold percentage for the battery's charge level.
    /// </summary>
    /// <remarks>
    /// This value is used to prevent the battery from overcharging.
    /// </remarks>
    public int ThresholdMax { get; set; }

    /// <summary>
    /// Gets or sets the start time for charging the battery from the grid.
    /// </summary>
    public TimeSpan ChargeGridStartTime { get; set; }

    /// <summary>
    /// Gets or sets the end time for charging the battery from the grid.
    /// </summary>
    public TimeSpan ChargeGridEndTime { get; set; }
}