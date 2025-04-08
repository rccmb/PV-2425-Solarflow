using System.Text.Json.Serialization;

namespace SolarflowClient.Models;

public class Forecast
{
    [JsonPropertyName("Id")] public int Id { get; set; }

    [JsonPropertyName("batteryId")] public int BatteryID { get; set; }

    [JsonPropertyName("forecastDate")] public DateTime ForecastDate { get; set; }

    [JsonPropertyName("kwh")] public double kwh { get; set; }

    [JsonPropertyName("solarHoursExpected")]
    public double SolarHoursExpected { get; set; }

    [JsonPropertyName("weatherCondition")] public string WeatherCondition { get; set; }
}