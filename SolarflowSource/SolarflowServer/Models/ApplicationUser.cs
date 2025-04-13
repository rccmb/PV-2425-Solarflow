using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using SolarflowServer.Models;

/// <summary>
///     Represents an application user with additional properties beyond the default IdentityUser.
/// </summary>
public class ApplicationUser : IdentityUser<int>
{
    /// <summary>
    ///     Gets or sets the full name of the user.
    /// </summary>
    public string Fullname { get; set; }

    /// <summary>
    ///     Gets or sets the URL or path to the user's profile photo.
    /// </summary>
    public string Photo { get; set; }

    /// <summary>
    ///     Gets or sets a value indicating whether the user's email has been confirmed.
    /// </summary>
    public bool ConfirmedEmail { get; set; } = false;

    /// <summary>
    ///     Gets or sets the date and time when the user account was created.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    ///     Gets or sets the battery associated with the user.
    /// </summary>
    public Battery Battery { get; set; }

    /// <summary>
    ///     Gets or sets the view account associated with the user.
    /// </summary>
    public ViewAccount ViewAccount { get; set; }

    /// <summary>
    ///     Gets or sets the latitude of the user's location.
    /// </summary>
    [Required]
    public double Latitude { get; set; }

    /// <summary>
    ///     Gets or sets the longitude of the user's location.
    /// </summary>
    [Required]
    public double Longitude { get; set; }


    /// <summary>
    ///     Gets or sets the amount of energy drawn from the grid in kilowatt-hours.
    /// </summary>
    [Required]
    public double GridKWh { get; set; }

    // Demo Columns
    [Required] public double SolarKWh { get; set; }


    // Navigation property for EnergyRecords (one-to-many)
    public ICollection<EnergyRecord> EnergyRecords { get; set; } = new List<EnergyRecord>();
}