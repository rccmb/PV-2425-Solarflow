using System.Text.Json.Serialization;

namespace SolarflowClient.Models;

public class Battery
{
    [JsonPropertyName("id")] public int Id { get; set; }

    [JsonPropertyName("chargeLevel")] public int ChargeLevel { get; set; }

    [JsonPropertyName("maxKW")] public int MaxKW { get; set; }

    [JsonPropertyName("chargingSource")] public string ChargingSource { get; set; }

    [JsonPropertyName("batteryMode")] public string BatteryMode { get; set; }

    [JsonPropertyName("minimalTreshold")] public int MinimalTreshold { get; set; }

    [JsonPropertyName("maximumTreshold")] public int MaximumTreshold { get; set; }

    [JsonPropertyName("spendingStartTime")]
    public string SpendingStartTime { get; set; }

    [JsonPropertyName("spendingEndTime")] public string SpendingEndTime { get; set; }

    [JsonPropertyName("lastupdate")] public string LastUpdate { get; set; }

    [JsonPropertyName("userid")] public int UserId { get; set; }
    [JsonPropertyName("user")] public string User { get; set; }
}