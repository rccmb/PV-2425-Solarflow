using System.ComponentModel.DataAnnotations;
using SolarflowClient.Models.Enums;

namespace SolarflowClient.Models.ViewModels.Battery;

public class GetBatteryViewModel
{
    public int ChargeLevel { get; set; }

    [Required] public BatterySource ChargeSource { get; set; }

    [Required] public BatteryMode ChargeMode { get; set; }

    [Required]
    [Range(0, 100, ErrorMessage = "Minimal Threshold must be between 0 and 100.")]
    public int ThresholdMin { get; set; }

    [Required]
    [Range(0, 100, ErrorMessage = "Maximum Threshold must be between 0 and 100.")]
    public int ThresholdMax { get; set; }

    [Required] public TimeSpan ChargeGridStartTime { get; set; }

    [Required] public TimeSpan ChargeGridEndTime { get; set; }

    public string? LastUpdate { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (ThresholdMax < ThresholdMin)
            yield return new ValidationResult("Maximum Threshold cannot be lower than Minimal Threshold",
                new[] { nameof(ThresholdMax) });
    }
}