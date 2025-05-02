namespace SolarflowClient.Models.ViewModels.Authentication
{
    using System.ComponentModel.DataAnnotations;

    /// <summary>
    /// Represents the data required to reset a user's password.
    /// </summary>
    public class ResetPasswordViewModel
    {
        /// <summary>
        /// Gets or sets the token used to verify the password reset request.
        /// </summary>
        /// <remarks>
        /// This property is required.
        /// </remarks>
        [Required]
        public string Token { get; set; }

        /// <summary>
        /// Gets or sets the email address associated with the account for which the password is being reset.
        /// </summary>
        /// <remarks>
        /// This property is required and must be in a valid email address format.
        /// </remarks>
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        /// <summary>
        /// Gets or sets the new password for the user's account.
        /// </summary>
        /// <remarks>
        /// This property is required, must be at least 6 characters long, and must be entered in a secure manner.
        /// </remarks>
        [Required]
        [DataType(DataType.Password)]
        [MinLength(6, ErrorMessage = "Password must be at least 6 characters.")]
        public string NewPassword { get; set; }

        /// <summary>
        /// Gets or sets the confirmation password for the user's account.
        /// </summary>
        /// <remarks>
        /// This property is required and must match the <see cref="NewPassword"/> property.
        /// </remarks>
        [Required]
        [DataType(DataType.Password)]
        [Compare("NewPassword", ErrorMessage = "Passwords do not match.")]
        public string ConfirmPassword { get; set; }
    }

}
