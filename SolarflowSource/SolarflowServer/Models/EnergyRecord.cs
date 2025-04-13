using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SolarflowServer.Models;

/// <summary>
///     Represents an energy record containing data about energy usage and production.
/// </summary>
public class EnergyRecord
{
    /// <summary>
    ///     Gets or sets the unique identifier for the energy record.
    /// </summary>
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    /// <summary>
    ///     Gets or sets the timestamp of when the energy record was created.
    /// </summary>
    [Required]
    public DateTime Timestamp { get; set; }

    /// <summary>
    ///     Gets or sets the amount of energy consumed by the house in kilowatt-hours.
    /// </summary>
    [Required]
    public double House { get; set; }

    /// <summary>
    ///     Gets or sets the amount of energy drawn from the grid in kilowatt-hours.
    /// </summary>
    [Required]
    public double Grid { get; set; }

    /// <summary>
    ///     Gets or sets the amount of energy produced by solar panels in kilowatt-hours.
    /// </summary>
    [Required]
    public double Solar { get; set; }

    /// <summary>
    ///     Gets or sets the amount of energy stored in or drawn from the battery in kilowatt-hours.
    /// </summary>
    [Required]
    public double Battery { get; set; }

    /// <summary>
    ///     Gets or sets the foreign key for the associated ApplicationUser.
    /// </summary>
    [ForeignKey("ApplicationUser")]
    public int ApplicationUserId { get; set; }

    /// <summary>
    ///     Gets or sets the associated ApplicationUser.
    /// </summary>
    public ApplicationUser ApplicationUser { get; set; }
}