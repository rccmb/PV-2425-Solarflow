using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SolarflowServer.Models;

/// <summary>
/// Represents a battery with its properties and configuration details.
/// </summary>
public class Battery
{
    /// <summary>
    /// Gets or sets the unique identifier for the battery.
    /// </summary>
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the current charge level of the battery.
    /// </summary>
    [Required] public int ChargeLevel { get; set; }

    /// <summary>
    /// Gets or sets the maximum power output of the battery in kilowatts.
    /// </summary>
    [Required] public int MaxKW { get; set; }

    /// <summary>
    /// Gets or sets the source used to charge the battery.
    /// </summary>
    [Required] public string ChargingSource { get; set; }

    /// <summary>
    /// Gets or sets the operational mode of the battery.
    /// </summary>
    [Required] public string BatteryMode { get; set; }

    /// <summary>
    /// Gets or sets the minimal threshold percentage for the battery's charge level.
    /// </summary>
    [Required]
    [Range(0, 100, ErrorMessage = "Minimal Threshold must be between 0 and 100.")]
    public int MinimalTreshold { get; set; }

    /// <summary>
    /// Gets or sets the maximum threshold percentage for the battery's charge level.
    /// </summary>
    [Required]
    [Range(0, 100, ErrorMessage = "Maximum Threshold must be between 0 and 100.")]
    public int MaximumTreshold { get; set; }

    /// <summary>
    /// Gets or sets the start time for energy spending from the battery.
    /// </summary>
    [Required] public string SpendingStartTime { get; set; }

    /// <summary>
    /// Gets or sets the end time for energy spending from the battery.
    /// </summary>
    [Required] public string SpendingEndTime { get; set; }

    /// <summary>
    /// Gets or sets the timestamp of the last update to the battery's data.
    /// </summary>
    public string? LastUpdate { get; set; }

    /// <summary>
    /// Gets or sets the unique identifier of the user associated with the battery.
    /// </summary>
    [ForeignKey("ApplicationUser")] public int UserId { get; set; }

    /// <summary>
    /// Gets or sets the application user associated with the battery.
    /// </summary>
    public ApplicationUser User { get; set; }

    /// <summary>
    /// Validates the battery's configuration to ensure the maximum threshold is not lower than the minimal threshold.
    /// </summary>
    /// <param name="validationContext">The context in which the validation is performed.</param>
    /// <returns>A collection of validation results.</returns>
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (MaximumTreshold < MinimalTreshold)
            yield return new ValidationResult("Maximum Threshold cannot be lower than Minimal Threshold",
                new[] { nameof(MaximumTreshold) });
    }
}