using System.Text.Json.Serialization;

namespace SolarflowClient.Models;

/// <summary>
/// Represents a battery with its properties and configuration details.
/// </summary>
public class Battery
{
    /// <summary>
    /// Gets or sets the unique identifier for the battery.
    /// </summary>
    [JsonPropertyName("id")] public int Id { get; set; }

    /// <summary>
    /// Gets or sets the current charge level of the battery.
    /// </summary>
    [JsonPropertyName("chargeLevel")] public int ChargeLevel { get; set; }

    /// <summary>
    /// Gets or sets the maximum power output of the battery in kilowatts.
    /// </summary>
    [JsonPropertyName("maxKW")] public int MaxKW { get; set; }

    /// <summary>
    /// Gets or sets the source used to charge the battery.
    /// </summary>
    [JsonPropertyName("chargingSource")] public string ChargingSource { get; set; }

    /// <summary>
    /// Gets or sets the operational mode of the battery.
    /// </summary>
    [JsonPropertyName("batteryMode")] public string BatteryMode { get; set; }

    /// <summary>
    /// Gets or sets the minimal threshold percentage for the battery's charge level.
    /// </summary>
    [JsonPropertyName("minimalTreshold")] public int MinimalTreshold { get; set; }

    /// <summary>
    /// Gets or sets the maximum threshold percentage for the battery's charge level.
    /// </summary>
    [JsonPropertyName("maximumTreshold")] public int MaximumTreshold { get; set; }

    /// <summary>
    /// Gets or sets the start time for energy spending from the battery.
    /// </summary>
    [JsonPropertyName("spendingStartTime")]
    public string SpendingStartTime { get; set; }

    /// <summary>
    /// Gets or sets the end time for energy spending from the battery.
    /// </summary>
    [JsonPropertyName("spendingEndTime")] public string SpendingEndTime { get; set; }

    /// <summary>
    /// Gets or sets the timestamp of the last update to the battery's data.
    /// </summary>
    [JsonPropertyName("lastupdate")] public string LastUpdate { get; set; }

    /// <summary>
    /// Gets or sets the unique identifier of the user associated with the battery.
    /// </summary>
    [JsonPropertyName("userid")] public int UserId { get; set; }

    /// <summary>
    /// Gets or sets the user associated with the battery.
    /// </summary>
    [JsonPropertyName("user")] public string User { get; set; }
}