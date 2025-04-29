namespace SolarflowServer.DTOs.Authentication
{
    /// <summary>
    /// Represents the data transfer object used for resetting a user's password.
    /// </summary>
    public class ResetPasswordDTO
    {
        /// <summary>
        /// Gets or sets the email address of the user requesting a password reset.
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// Gets or sets the token used to verify the password reset request.
        /// </summary>
        public string Token { get; set; }

        /// <summary>
        /// Gets or sets the new password for the user's account.
        /// </summary>
        public string NewPassword { get; set; }

        /// <summary>
        /// Gets or sets the confirmation of the new password to ensure it matches.
        /// </summary>
        public string ConfirmPassword { get; set; }
    }

}
