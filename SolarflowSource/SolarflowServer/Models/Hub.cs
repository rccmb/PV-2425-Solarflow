using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SolarflowServer.Models;

/// <summary>
/// Represents a hub that tracks energy data and is associated with a user and a battery.
/// </summary>
public class Hub
{
    /// <summary>
    /// Gets or sets the unique identifier for the hub.
    /// </summary>
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]

    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the unique identifier of the user associated with the hub.
    /// </summary>
    [Required] public int UserId { get; set; }

    /// <summary>
    /// Gets or sets the user associated with the hub.
    /// </summary>
    [ForeignKey(nameof(UserId))] public virtual ApplicationUser User { get; set; }

    /// <summary>
    /// Gets or sets the latitude of the hub's location.
    /// </summary>
    [Required] public double Latitude { get; set; }

    /// <summary>
    /// Gets or sets the longitude of the hub's location.
    /// </summary>
    [Required] public double Longitude { get; set; }

    /// <summary>
    /// Gets or sets the amount of energy drawn from the grid in kilowatt-hours.
    /// </summary>
    [Required] public double GridKWh { get; set; }

    /// <summary>
    /// Gets or sets the unique identifier of the battery associated with the hub.
    /// </summary>
    [Required] public int BatteryId { get; set; }

    /// <summary>
    /// Gets or sets the battery associated with the hub.
    /// </summary>
    [ForeignKey(nameof(BatteryId))] public virtual Battery Battery { get; set; }

    /// <summary>
    /// Gets or sets the collection of energy records associated with the hub.
    /// </summary>
    public virtual ICollection<EnergyRecord> EnergyRecords { get; set; }

    // Demo Columns
    [Required] public double DemoSolar { get; set; }

    [Required] public double DemoConsumption { get; set; }

    [Required] public int DemoPeople { get; set; }
}