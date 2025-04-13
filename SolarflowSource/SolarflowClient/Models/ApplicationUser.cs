using Microsoft.AspNetCore.Identity;
using SolarflowClient.Models;

/// <summary>
/// Represents an application user with additional properties beyond the default IdentityUser.
/// </summary>
public class ApplicationUser : IdentityUser<int>
{
    /// <summary>
    /// Gets or sets the full name of the user.
    /// </summary>
    public string Fullname { get; set; }

    /// <summary>
    /// Gets or sets the URL or path to the user's profile photo.
    /// </summary>
    public string Photo { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the user's email has been confirmed.
    /// </summary>
    public bool ConfirmedEmail { get; set; } = false;

    /// <summary>
    /// Gets or sets the date and time when the user account was created.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets the battery associated with the user.
    /// </summary>
    public Battery Battery { get; set; }

    /// <summary>
    /// Gets or sets the view account associated with the user.
    /// </summary>
    public ViewAccount ViewAccount { get; set; }

    /// <summary>
    /// Gets or sets the collection of hubs associated with the user.
    /// </summary>
    public virtual ICollection<Hub> Hubs { get; set; }
}