using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SolarflowServer.Models;

/// <summary>
///     Represents an energy record containing data about energy usage and production.
/// </summary>
public class EnergyRecord
{
    private double _battery;

    private double _grid;

    private double _house;

    private double _solar;

    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Required] public DateTime Timestamp { get; set; }

    /// <summary>
    ///     Gets or sets the amount of energy consumed by the house in kilowatt-hours.
    /// </summary>
    [Required]
    public double House
    {
        get => _house;
        set => _house = Math.Round(value, 2);
    }

    /// <summary>
    ///     Gets or sets the amount of energy drawn from the grid in kilowatt-hours.
    /// </summary>
    [Required]
    public double Grid
    {
        get => _grid;
        set => _grid = Math.Round(value, 2);
    }

    /// <summary>
    ///     Gets or sets the amount of energy produced by solar panels in kilowatt-hours.
    /// </summary>
    [Required]
    public double Solar
    {
        get => _solar;
        set => _solar = Math.Round(value, 2);
    }

    /// <summary>
    ///     Gets or sets the amount of energy stored in or drawn from the battery in kilowatt-hours.
    /// </summary>
    [Required]
    public double Battery
    {
        get => _battery;
        set => _battery = Math.Round(value, 2);
    }

    [ForeignKey("ApplicationUser")] public int ApplicationUserId { get; set; }

    public ApplicationUser ApplicationUser { get; set; }
}