using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace SolarflowClient.Models;

public class ViewAccount : IdentityUser<int>
{
    public string Name { get; set; }

    [ForeignKey("ApplicationUser")] public int UserId { get; set; }

    public ApplicationUser User { get; set; }
}