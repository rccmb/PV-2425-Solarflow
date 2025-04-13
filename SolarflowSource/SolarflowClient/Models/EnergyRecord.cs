using System.Text.Json.Serialization;

namespace SolarflowClient.Models;

/// <summary>
/// Represents an energy record containing data about energy usage and production.
/// </summary>
public class EnergyRecord
{
    /// <summary>
    /// Gets or sets the unique identifier for the energy record.
    /// </summary>
    [JsonPropertyName("id")] public int Id { get; set; }

    /// <summary>
    /// Gets or sets the unique identifier of the hub associated with this energy record.
    /// </summary>
    [JsonPropertyName("hubid")] public int HubId { get; set; }

    /// <summary>
    /// Gets or sets the timestamp of when the energy record was created.
    /// </summary>
    [JsonPropertyName("timestamp")] public DateTime Timestamp { get; set; }

    /// <summary>
    /// Gets or sets the amount of energy consumed by the house in kilowatt-hours.
    /// </summary>
    [JsonPropertyName("house")] public double House { get; set; }

    /// <summary>
    /// Gets or sets the amount of energy drawn from the grid in kilowatt-hours.
    /// </summary>
    [JsonPropertyName("grid")] public double Grid { get; set; }

    /// <summary>
    /// Gets or sets the amount of energy produced by solar panels in kilowatt-hours.
    /// </summary>
    [JsonPropertyName("solar")] public double Solar { get; set; }

    /// <summary>
    /// Gets or sets the amount of energy stored in or drawn from the battery in kilowatt-hours.
    /// </summary>
    [JsonPropertyName("battery")] public double Battery { get; set; }
}