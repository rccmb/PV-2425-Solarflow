using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SolarflowServer.Models;

public class Hub
{
    [Key] public int Id { get; set; }

    [Required] public int UserId { get; set; }

    [ForeignKey(nameof(UserId))] public virtual ApplicationUser User { get; set; }

    [Required] public float GridKWh { get; set; }

    [Required] public float SolarKWh { get; set; }

    [Required] public int BatteryId { get; set; }

    [ForeignKey(nameof(BatteryId))] public virtual Battery Battery { get; set; }

    public virtual ICollection<EnergyRecord> EnergyRecords { get; set; }
}