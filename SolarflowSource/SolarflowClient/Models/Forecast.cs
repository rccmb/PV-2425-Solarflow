using System.Text.Json.Serialization;

namespace SolarflowClient.Models;

/// <summary>
/// Represents a forecast for energy production and weather conditions.
/// </summary>
public class Forecast
{
    /// <summary>
    /// Gets or sets the unique identifier for the forecast.
    /// </summary>
    [JsonPropertyName("Id")] public int Id { get; set; }

    /// <summary>
    /// Gets or sets the unique identifier of the battery associated with the forecast.
    /// </summary>
    [JsonPropertyName("batteryId")] public int BatteryID { get; set; }

    /// <summary>
    /// Gets or sets the date for which the forecast is made.
    /// </summary>
    [JsonPropertyName("forecastDate")] public DateTime ForecastDate { get; set; }

    /// <summary>
    /// Gets or sets the expected energy production in kilowatt-hours.
    /// </summary>
    [JsonPropertyName("kwh")] public double kwh { get; set; }

    /// <summary>
    /// Gets or sets the expected number of solar hours for the forecast date.
    /// </summary>
    [JsonPropertyName("solarHoursExpected")]
    public double SolarHoursExpected { get; set; }

    /// <summary>
    /// Gets or sets the expected weather condition for the forecast date.
    /// </summary>
    [JsonPropertyName("weatherCondition")] public string WeatherCondition { get; set; }
}