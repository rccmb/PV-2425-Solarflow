namespace SolarflowClient.Models.Enums;

/// <summary>
/// Represents the source used to charge the battery.
/// </summary>
public enum BatterySource
{
    /// <summary>
    /// The battery can be charged from all available sources.
    /// </summary>
    All,

    /// <summary>
    /// The battery is charged using energy from the grid.
    /// </summary>
    Grid,

    /// <summary>
    /// The battery is charged using energy from solar panels.
    /// </summary>
    Solar
}