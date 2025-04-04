using System.Text.Json.Serialization;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum EnergySource
{
    Consumption = 0,
    Grid = 1,
    Solar = 2,
    Battery = 3
}