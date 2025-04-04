using Microsoft.AspNetCore.Identity;
using SolarflowServer.Models;

public class ApplicationUser : IdentityUser<int>
{
    public string Fullname { get; set; }

    public string Photo { get; set; }

    public bool ConfirmedEmail { get; set; } = false;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Battery Battery { get; set; }

    public ViewAccount ViewAccount { get; set; }

    public virtual ICollection<Hub> Hubs { get; set; }
}