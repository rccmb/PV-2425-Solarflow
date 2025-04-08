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

    [Required] public double House { get; set; }

    [Required] public double Grid { get; set; }

    [Required] public double Solar { get; set; }

    [Required] public double Battery { get; set; }
}