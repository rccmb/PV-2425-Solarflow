using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using SolarflowClient.Models.Enums;

namespace SolarflowClient.Models;

/// <summary>
///     Represents a battery with its properties and configuration details.
/// </summary>
public class Battery
{
    [JsonPropertyName("id")] public int Id { get; set; }


    [JsonPropertyName("capacity")] public double Capacity { get; set; }


    [JsonPropertyName("capacityMax")] public double CapacityMax { get; set; }


    public int CapacityLevel
    {
        get => CapacityMax == 0 ? 0 : (int)(Capacity / CapacityMax * 100);
        set
        {
            var percentage = value < 0 ? 0 : value > 100 ? 100 : value;
            Capacity = percentage / 100.0 * CapacityMax;
        }
    }


    [JsonPropertyName("chargeRate")] public double ChargeRate { get; set; } = 5.0;

    [JsonPropertyName("dischargeRate")] public double DischargeRate { get; set; } = 7.0;


    [JsonPropertyName("chargeMode")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public BatteryMode ChargeMode { get; set; }

    [JsonPropertyName("chargeSource")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public BatterySource ChargeSource { get; set; }


    [JsonPropertyName("thresholdMin")] public int ThresholdMin { get; set; }

    [JsonPropertyName("thresholdMax")] public int ThresholdMax { get; set; }


    [JsonPropertyName("chargeGridStartTime")]

    public TimeSpan ChargeGridStartTime { get; set; }

    [JsonPropertyName("chargeGridEndTime")]

    public TimeSpan ChargeGridEndTime { get; set; }


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


    [JsonPropertyName("lastUpdate")] public DateTime LastUpdate { get; set; }


    [JsonPropertyName("userId")] public int UserId { get; set; }

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