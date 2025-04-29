namespace SolarflowClient.Models;

/// <summary>
/// Represents the data required to display the home dashboard, including energy records, battery details, and forecasts.
/// </summary>
public class HomeViewModel
{
    /// <summary>
    /// Gets or sets the most recent energy record.
    /// </summary>
    public EnergyRecord? LastEnergyRecord { get; set; }

    /// <summary>
    /// Gets or sets the collection of energy records for the specified date range.
    /// </summary>
    public List<EnergyRecord>? EnergyRecords { get; set; }

    /// <summary>
    /// Gets or sets the details of the user's battery.
    /// </summary>
    public Battery? Battery { get; set; }

    /// <summary>
    /// Gets or sets the collection of weather forecasts.
    /// </summary>
    public List<Forecast>? Forecast { get; set; }

    /// <summary>
    /// Gets or sets the filter criteria for querying energy records.
    /// </summary>
    public EnergyRecordFilter Filter { get; set; } = new();
}