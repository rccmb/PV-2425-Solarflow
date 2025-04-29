namespace SolarflowClient.Models.Enums;

/// <summary>
/// Represents the operational modes of a battery.
/// </summary>
public enum BatteryMode
{
    /// <summary>
    /// The battery operates in a normal mode with default settings.
    /// </summary>
    Normal,

    /// <summary>
    /// The battery operates in a personalized mode with user-defined settings.
    /// </summary>
    Personalized,

    /// <summary>
    /// The battery operates in an emergency mode, prioritizing critical operations.
    /// </summary>
    Emergency
}