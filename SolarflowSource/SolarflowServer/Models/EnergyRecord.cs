using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SolarflowServer.Models;

public class EnergyRecord
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Required] public int HubId { get; set; }

    [ForeignKey("HubId")] public virtual Hub Hub { get; set; }

    [Required] public DateTime Timestamp { get; set; }

    [Required] public float Consumption { get; set; }

    [Required] public float Grid { get; set; }

    [Required] public float Solar { get; set; }

    [Required] public float Battery { get; set; }
}