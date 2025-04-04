using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SolarflowServer.Models;

public class EnergyRecord
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Required] public DateTime Timestamp { get; set; } // Timestamp for the energy record

    [Required] public float Value { get; set; } // Energy value (e.g., kWh)

    [Required] public EnergySource Source { get; set; } // Source of the energy (e.g., Solar, Battery)

    [Required] public EnergySource Target { get; set; } // Target of the energy (e.g., Consumption, Grid)

    [ForeignKey("Hub")] public int HubId { get; set; } // Link to the Hub that the record is associated with

    public virtual Hub Hub { get; set; }

}