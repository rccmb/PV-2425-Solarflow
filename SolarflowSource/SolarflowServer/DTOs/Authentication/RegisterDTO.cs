namespace SolarflowServer.DTOs.Authentication;

/// <summary>
/// Represents the data transfer object used for user registration.
/// </summary>
public class RegisterDTO
{
    /// <summary>
    /// Gets or sets the full name of the user registering an account.
    /// </summary>
    public string Fullname { get; set; }

    /// <summary>
    /// Gets or sets the email address of the user registering an account.
    /// </summary>
    public string Email { get; set; }

    /// <summary>
    /// Gets or sets the password for the user's account.
    /// </summary>
    public string Password { get; set; }

    /// <summary>
    /// Gets or sets the latitude of the user's location. This value is optional.
    /// </summary>
    public double? Latitude { get; set; }

    /// <summary>
    /// Gets or sets the longitude of the user's location. This value is optional.
    /// </summary>
    public double? Longitude { get; set; }
}