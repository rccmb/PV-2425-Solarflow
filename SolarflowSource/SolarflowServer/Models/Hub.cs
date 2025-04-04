using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SolarflowServer.Models;

public class Hub(ApplicationDbContext context)
{
    [Key] [ForeignKey("User")] public int ApplicationUserId { get; set; }

    public ApplicationUser User { get; set; }

    [Required] public float GridKW { get; set; }

    [Required] public float SolarArea { get; set; }

    [Required] public Battery Battery { get; set; }

    public ICollection<EnergyRecord> EnergyRecords { get; set; } = new List<EnergyRecord>();
}