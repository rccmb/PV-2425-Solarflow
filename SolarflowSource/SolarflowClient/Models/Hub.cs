using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SolarflowClient.Models;

public class Hub
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]

    public int Id { get; set; }

    [Required] public int UserId { get; set; }

    [ForeignKey(nameof(UserId))] public virtual ApplicationUser User { get; set; }

    [Required] public double Latitude { get; set; }

    [Required] public double Longitude { get; set; }

    [Required] public double GridKWh { get; set; }

    [Required] public int BatteryId { get; set; }

    [ForeignKey(nameof(BatteryId))] public virtual Battery Battery { get; set; }

    public virtual ICollection<EnergyRecord> EnergyRecords { get; set; }

    // Demo Columns
    [Required] public double DemoSolar { get; set; }

    [Required] public double DemoConsumption { get; set; }

    [Required] public int DemoPeople { get; set; }
}