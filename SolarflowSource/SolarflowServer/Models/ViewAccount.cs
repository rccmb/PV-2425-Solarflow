using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


public class ViewAccount : IdentityUser<int>
{

    public string Name { get; set; } 

    [ForeignKey("ApplicationUser")]
    public int UserId { get; set; }

    public ApplicationUser User { get; set; } 
}