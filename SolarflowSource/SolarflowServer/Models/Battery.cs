using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SolarflowServer.Models.Enums;

namespace SolarflowServer.Models;

/// <summary>
///     Represents a battery with its properties and configuration details.
/// </summary>
public class Battery : IValidatableObject
{
    private double _capacity;


    private double _capacityMax = 14.40;

    /// <summary>
    ///     Gets or sets the unique identifier for the battery.
    /// </summary>
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    /// <summary>
    ///     Gets or sets the current capacity of the battery in kilowatts.
    ///     Should be a value between 0.0 and CapacityMax.
    /// </summary>
    [Required]
    public double Capacity
    {
        get => _capacity;
        set => _capacity = value > CapacityMax ? CapacityMax : value;
    }

    /// <summary>
    ///     Gets or sets the maximum capacity of the battery in kilowatts.
    /// </summary>
    [Required]
    public double CapacityMax
    {
        get => _capacityMax;
        set
        {
            _capacityMax = value;
            // If the current capacity exceeds the new maximum, clamp it to the new maximum.
            if (Capacity > _capacityMax) Capacity = _capacityMax;
        }
    }


    /// <summary>
    ///     Gets the capacity level as a percentage of CapacityMax.
    ///     The setter adjusts Capacity according to the percentage.
    /// </summary>
    /// <summary>
    ///     Gets or sets the charge rate in kilowatts per hour.
    /// </summary>
    [Required]
    public double ChargeRate { get; set; } = 4.5;

    /// <summary>
    ///     Gets or sets the discharge rate in kilowatts per hour.
    /// </summary>
    [Required]
    public double DischargeRate { get; set; } = 6.5;


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
    ///     Gets the available quota for charging at this instant.
    ///     Computed as the lesser of the remaining capacity (based on ThresholdMax) and the ChargeRate.
    ///     Not mapped to the database.
    /// </summary>
    [NotMapped]
    public double QuotaCharge
    {
        get
        {
            var charge = new List<double>
                { ChargeRate, CapacityMax - Capacity };

            if (ChargeMode == BatteryMode.Personalized && CapacityLevel >= ThresholdMax) return 0;

            if (ChargeMode == BatteryMode.Personalized && CapacityLevel < ThresholdMax)
                charge.Add(Math.Max(0.0, CapacityMax / 100 * ThresholdMax - Capacity));

            return charge.Min();
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
            var discharge = new List<double>
                { DischargeRate, Capacity };

            if (ChargeMode == BatteryMode.Personalized && CapacityLevel <= ThresholdMin) return 0;

            if (ChargeMode == BatteryMode.Personalized && CapacityLevel > ThresholdMin)
                discharge.Add(Math.Max(0.0, Capacity - CapacityMax / 100 * ThresholdMin));

            return discharge.Min();
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