using Microsoft.AspNetCore.Identity;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SolarflowServer.Models
{
    public class Battery
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }

        [Required]
        public int ChargeLevel { get; set; }

        [Required]
        public string ChargingSource { get; set; }

        [Required]
        public string BatteryMode { get; set; }

        [Required]
        [Range(0, 100, ErrorMessage = "Minimal Threshold must be between 0 and 100.")]
        public int MinimalTreshold { get; set; }

        [Required]
        [Range(0, 100, ErrorMessage = "Maximum Threshold must be between 0 and 100.")]
        public int MaximumTreshold { get; set; }

        [Required]
        public string SpendingStartTime { get; set; }

        [Required]
        public string SpendingEndTime { get; set; }

        public string? LastUpdate { get; set; }

        [ForeignKey("ApplicationUser")]
        public int UserId { get; set; }

        public ApplicationUser User { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (MaximumTreshold < MinimalTreshold)
            {
                yield return new ValidationResult("Maximum Threshold cannot be lower than Minimal Threshold", new[] { nameof(MaximumTreshold) });
            }
        }
    }
}
