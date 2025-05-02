using Newtonsoft.Json;

namespace SolarflowClient.Models.ViewModels.Authentication;

/// <summary>
/// Represents the data returned when retrieving user details.
/// </summary>
public class GetUserViewModel
{
    /// <summary>
    /// Gets or sets the full name of the user.
    /// </summary>
    [JsonProperty("fullname")] public string Fullname { get; set; }

    /// <summary>
    /// Gets or sets the URL or path to the user's profile photo.
    /// </summary>
    [JsonProperty("photo")] public string Photo { get; set; }

    /// <summary>
    /// Gets or sets the email address of the user.
    /// </summary>
    [JsonProperty("email")] public string Email { get; set; }

    /// <summary>
    /// Gets or sets the date and time when the user account was created.
    /// </summary>
    [JsonProperty("createdAt")] public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the user has a view-only account associated with their profile.
    /// </summary>
    [JsonProperty("hasViewAccount")] public bool HasViewAccount { get; set; }

    /// <summary>
    /// Gets or sets the total energy drawn from the grid in kilowatt-hours.
    /// </summary>
    [JsonProperty("gridKWh")] public double GridKWh { get; set; }

    /// <summary>
    /// Gets or sets the total energy produced by solar panels in kilowatt-hours.
    /// </summary>
    [JsonProperty("solarKWh")] public double SolarKWh { get; set; }

    /// <summary>
    /// Gets or sets the latitude of the user's location.
    /// </summary>
    [JsonProperty("latitude")] public double Latitude { get; set; }

    /// <summary>
    /// Gets or sets the longitude of the user's location.
    /// </summary>
    [JsonProperty("longitude")] public double Longitude { get; set; }
}