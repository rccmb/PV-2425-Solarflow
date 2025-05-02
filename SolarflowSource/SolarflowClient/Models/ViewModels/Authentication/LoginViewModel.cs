using System.ComponentModel.DataAnnotations;

namespace SolarflowClient.Models.ViewModels.Authentication
{
    /// <summary>
    /// Represents the data required for a user to log in to the application.
    /// </summary>
    public class LoginViewModel
    {
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
        /// Gets or sets the password of the user.
        /// </summary>
        /// <remarks>
        /// This property is required and must be entered in a secure manner.
        /// </remarks>
        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the user wants to remain logged in across sessions.
        /// </summary>
        public bool RememberMe { get; set; } = false;
    }
}
