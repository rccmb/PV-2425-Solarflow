using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

/// <summary>
/// Represents a view account associated with an application user.
/// </summary>
public class ViewAccount : IdentityUser<int>
{
    /// <summary>
    /// Gets or sets the name of the view account.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Gets or sets the unique identifier of the application user associated with this view account.
    /// </summary>
    [ForeignKey("ApplicationUser")]
    public int UserId { get; set; }

    /// <summary>
    /// Gets or sets the application user associated with this view account.
    /// </summary>
    public ApplicationUser User { get; set; } 
}