using System.ComponentModel.DataAnnotations;

namespace SolarflowClient.Models.ViewModels.Authentication
{
    /// <summary>
    /// Represents the data required for initiating an account recovery process.
    /// </summary>
    public class AccountRecoveryViewModel
    {
        /// <summary>
        /// Gets or sets the email address associated with the account to be recovered.
        /// </summary>
        /// <remarks>
        /// This property is required and must be in a valid email address format.
        /// </remarks>
        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }
}
