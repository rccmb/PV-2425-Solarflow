using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using SolarflowClient.Models.Enums;

namespace SolarflowClient.Models;

/// <summary>
///     Represents a battery with its properties and configuration details.
/// </summary>
public class Battery
{
    /// <summary>
    /// Gets or sets the unique identifier for the battery.
    /// </summary>
    [JsonPropertyName("id")] public int Id { get; set; }

    /// <summary>
    /// Gets or sets the current capacity of the battery in kilowatt-hours.
    /// </summary>
    [JsonPropertyName("capacity")] public double Capacity { get; set; }

    /// <summary>
    /// Gets or sets the maximum capacity of the battery in kilowatt-hours.
    /// </summary>
    [JsonPropertyName("capacityMax")] public double CapacityMax { get; set; }

    /// <summary>
    /// Gets or sets the capacity level as a percentage of the maximum capacity.
    /// Adjusts the <see cref="Capacity"/> based on the percentage value.
    /// </summary>
    public int CapacityLevel
    {
        get => CapacityMax == 0 ? 0 : (int)(Capacity / CapacityMax * 100);
        set
        {
            var percentage = value < 0 ? 0 : value > 100 ? 100 : value;
            Capacity = percentage / 100.0 * CapacityMax;
        }
    }

    /// <summary>
    /// Gets or sets the charge rate of the battery in kilowatts per hour.
    /// </summary>
    [JsonPropertyName("chargeRate")] public double ChargeRate { get; set; } = 5.0;

    /// <summary>
    /// Gets or sets the discharge rate of the battery in kilowatts per hour.
    /// </summary>
    [JsonPropertyName("dischargeRate")] public double DischargeRate { get; set; } = 7.0;

    /// <summary>
    /// Gets or sets the operational charge mode of the battery.
    /// </summary>
    [JsonPropertyName("chargeMode")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public BatteryMode ChargeMode { get; set; }

    /// <summary>
    /// Gets or sets the source used to charge the battery.
    /// </summary>
    [JsonPropertyName("chargeSource")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public BatterySource ChargeSource { get; set; }

    /// <summary>
    /// Gets or sets the minimal threshold percentage for the battery's charge level.
    /// </summary>
    [JsonPropertyName("thresholdMin")] public int ThresholdMin { get; set; }

    /// <summary>
    /// Gets or sets the maximum threshold percentage for the battery's charge level.
    /// </summary>
    [JsonPropertyName("thresholdMax")] public int ThresholdMax { get; set; }

    /// <summary>
    /// Gets or sets the start time for energy spending (charging grid) as a time-of-day.
    /// </summary>
    [JsonPropertyName("chargeGridStartTime")]
    public TimeSpan ChargeGridStartTime { get; set; }

    /// <summary>
    /// Gets or sets the end time for energy spending (charging grid) as a time-of-day.
    /// </summary>
    [JsonPropertyName("chargeGridEndTime")]
    public TimeSpan ChargeGridEndTime { get; set; }

    /// <summary>
    /// Gets the available quota for charging at this instant.
    /// Computed as the lesser of the remaining capacity (based on <see cref="ThresholdMax"/>) and the <see cref="ChargeRate"/>.
    /// </summary>
    public double QuotaCharge
    {
        get
        {
            var allowedCapacity = ThresholdMax / 100.0 * CapacityMax;
            var remainingCapacity = allowedCapacity - Capacity;
            if (remainingCapacity <= 0)
                return 0;
            return remainingCapacity < ChargeRate ? remainingCapacity : ChargeRate;
        }
    }

    /// <summary>
    /// Gets the available quota for discharging at this instant.
    /// Computed as the lesser of the dischargeable energy (current capacity minus allowed minimum based on <see cref="ThresholdMin"/>)
    /// and the <see cref="DischargeRate"/>.
    /// </summary>
    public double QuotaDischarge
    {
        get
        {
            var allowedMinimum = ThresholdMin / 100.0 * CapacityMax;
            var dischargeable = Capacity - allowedMinimum;
            if (dischargeable <= 0)
                return 0;
            return dischargeable < DischargeRate ? dischargeable : DischargeRate;
        }
    }

    /// <summary>
    /// Gets or sets the timestamp of the last update to the battery's data.
    /// </summary>
    [JsonPropertyName("lastUpdate")] public DateTime LastUpdate { get; set; }

    /// <summary>
    /// Gets or sets the unique identifier of the user associated with the battery.
    /// </summary>
    [JsonPropertyName("userId")] public int UserId { get; set; }

    /// <summary>
    /// Gets or sets the application user associated with the battery.
    /// </summary>
    [JsonPropertyName("user")] public ApplicationUser User { get; set; }

    /// <summary>
    ///     Validates the battery configuration.
    /// </summary>
    /// <param name="validationContext">The context in which validation is performed.</param>
    /// <returns>A collection of validation results.</returns>
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (ThresholdMax < ThresholdMin)
            yield return new ValidationResult("Maximum Threshold cannot be lower than Minimal Threshold",
                new[] { nameof(ThresholdMax) });
    }
}