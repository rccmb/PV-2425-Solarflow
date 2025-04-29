using System.ComponentModel.DataAnnotations;

namespace SolarflowClient.Models.ViewModels.Authentication
{
    /// <summary>
    /// Represents the data required for a user to register a new account.
    /// </summary>
    public class RegisterViewModel
    {
        /// <summary>
        /// Gets or sets the full name of the user.
        /// </summary>
        /// <remarks>
        /// This property is required.
        /// </remarks>
        [Required]
        public string Fullname { get; set; }

        /// <summary>
        /// Gets or sets the email address of the user.
        /// </summary>
        /// <remarks>
        /// This property is required and must be in a valid email address format.
        /// </remarks>
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        /// <summary>
        /// Gets or sets the password for the user's account.
        /// </summary>
        /// <remarks>
        /// This property is required and must be entered in a secure manner.
        /// </remarks>
        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        /// <summary>
        /// Gets or sets the confirmation password for the user's account.
        /// </summary>
        /// <remarks>
        /// This property is required and must match the <see cref="Password"/> property.
        /// </remarks>
        [Required]
        [Compare("Password", ErrorMessage = "Passwords do not match.")]
        [DataType(DataType.Password)]
        public string ConfirmPassword { get; set; }

        /// <summary>
        /// Gets or sets the latitude of the user's location.
        /// </summary>
        /// <remarks>
        /// This property is optional and can be null.
        /// </remarks>
        public double? Latitude { get; set; }

        /// <summary>
        /// Gets or sets the longitude of the user's location.
        /// </summary>
        /// <remarks>
        /// This property is optional and can be null.
        /// </remarks>
        public double? Longitude { get; set; }

    }
}
