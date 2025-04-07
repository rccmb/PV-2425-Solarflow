using System.Text.Json.Serialization;

namespace SolarflowClient.Models;

public class EnergyRecord
{
    [JsonPropertyName("id")] public int Id { get; set; }

    [JsonPropertyName("hubid")] public int HubId { get; set; }

    [JsonPropertyName("timestamp")] public DateTime Timestamp { get; set; }

    [JsonPropertyName("house")] public double House { get; set; }

    [JsonPropertyName("grid")] public double Grid { get; set; }

    [JsonPropertyName("solar")] public double Solar { get; set; }

    [JsonPropertyName("battery")] public double Battery { get; set; }
}