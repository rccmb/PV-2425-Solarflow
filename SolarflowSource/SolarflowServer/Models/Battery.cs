using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SolarflowServer.Models.Enums;

namespace SolarflowServer.Models;

/// <summary>
///     Represents a battery with its properties and configuration details.
/// </summary>
public class Battery: IValidatableObject
{
    /// <summary>
    ///     Gets or sets the unique identifier for the battery.
    /// </summary>
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }


    // TODO: set to allow only up-to CapacityMax
    /// <summary>
    ///     Gets or sets the current capacity of the battery in kilowatts.
    ///     Should be a value between 0.0 and CapacityMax.
    /// </summary>
    [Required]
    public double Capacity { get; set; }


    // TODO: Set current Capacity to CapacityMax if Capacity > CapacityMax
    /// <summary>
    ///     Gets or sets the maximum capacity of the battery in kilowatts.
    /// </summary>
    [Required]
    public double CapacityMax { get; set; }


    /// <summary>
    ///     Gets the capacity level as a percentage of CapacityMax.
    ///     The setter adjusts Capacity according to the percentage.
    /// </summary>
    [NotMapped]
    public int CapacityLevel
    {
        get => CapacityMax == 0 ? 0 : (int)(Capacity / CapacityMax * 100);
        set
        {
            var percentage = value < 0 ? 0 : value > 100 ? 100 : value;
            Capacity = percentage / 100.0 * CapacityMax;
        }
    }


    /// <summary>
    ///     Gets or sets the charge rate in kilowatts per hour.
    /// </summary>
    [Required]
    public double ChargeRate { get; set; } = 5.0;

    /// <summary>
    ///     Gets or sets the discharge rate in kilowatts per hour.
    /// </summary>
    [Required]
    public double DischargeRate { get; set; } = 7.0;


    /// <summary>
    ///     Gets or sets the operational charge mode.
    /// </summary>
    [Required]
    public BatteryMode ChargeMode { get; set; }

    /// <summary>
    ///     Gets or sets the source used to charge the battery.
    /// </summary>
    [Required]
    public BatterySource ChargeSource { get; set; }


    /// <summary>
    ///     Gets or sets the minimal threshold percentage for the battery's charge level.
    /// </summary>
    [Required]
    [Range(0, 100, ErrorMessage = "Minimal Threshold must be between 0 and 100.")]
    public int ThresholdMin { get; set; }

    /// <summary>
    ///     Gets or sets the maximum threshold percentage for the battery's charge level.
    /// </summary>
    [Required]
    [Range(0, 100, ErrorMessage = "Maximum Threshold must be between 0 and 100.")]
    public int ThresholdMax { get; set; }


    /// <summary>
    ///     Gets or sets the start time for energy spending (charging grid) as a time-of-day.
    /// </summary>
    [Required]
    [Column(TypeName = "time")]
    public TimeSpan ChargeGridStartTime { get; set; }

    /// <summary>
    ///     Gets or sets the end time for energy spending (charging grid) as a time-of-day.
    /// </summary>
    [Required]
    [Column(TypeName = "time")]
    public TimeSpan ChargeGridEndTime { get; set; }


    /// <summary>
    ///     Gets the available quota for charging at this instant.
    ///     Computed as the lesser of the remaining capacity (based on ThresholdMax) and the ChargeRate.
    ///     Not mapped to the database.
    /// </summary>
    [NotMapped]
    public double QuotaCharge
    {
        get
        {
            var allowedCapacity = ThresholdMax / 100.0 * CapacityMax;
            var remainingCapacity = allowedCapacity - Capacity;
            if (remainingCapacity <= 0)
                return 0;
            return remainingCapacity < ChargeRate ? remainingCapacity : ChargeRate;
        }
    }


    /// <summary>
    ///     Gets the available quota for discharging at this instant.
    ///     Computed as the lesser of the dischargeable energy (current capacity minus allowed minimum based on ThresholdMin)
    ///     and DischargeRate.
    ///     Not mapped to the database.
    /// </summary>
    [NotMapped]
    public double QuotaDischarge
    {
        get
        {
            var allowedMinimum = ThresholdMin / 100.0 * CapacityMax;
            var dischargeable = Capacity - allowedMinimum;
            if (dischargeable <= 0)
                return 0;
            return dischargeable < DischargeRate ? dischargeable : DischargeRate;
        }
    }


    /// <summary>
    ///     Gets or sets the timestamp of the last update to the battery's data.
    /// </summary>
    public DateTime LastUpdate { get; set; }


    /// <summary>
    ///     Gets or sets the unique identifier of the user associated with the battery.
    /// </summary>
    [ForeignKey("ApplicationUser")]
    public int UserId { get; set; }

    /// <summary>
    ///     Gets or sets the application user associated with the battery.
    /// </summary>
    public ApplicationUser User { get; set; }

    /// <summary>
    ///     Validates the battery configuration.
    /// </summary>
    /// <param name="validationContext">The context in which validation is performed.</param>
    /// <returns>A collection of validation results.</returns>
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (ThresholdMax < ThresholdMin)
            yield return new ValidationResult("Maximum Threshold cannot be lower than Minimal Threshold",
                new[] { nameof(ThresholdMax) });
    }
}