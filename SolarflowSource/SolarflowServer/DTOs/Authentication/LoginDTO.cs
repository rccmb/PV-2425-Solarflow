namespace SolarflowServer.DTOs.Authentication
{
    /// <summary>
    /// Represents the data transfer object used for user login.
    /// </summary>
    public class LoginDTO
    {
        /// <summary>
        /// Gets or sets the email address of the user attempting to log in.
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// Gets or sets the password of the user attempting to log in.
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the user should remain logged in across sessions.
        /// </summary>
        public bool RememberMe { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the user has administrative privileges.
        /// </summary>
        public bool isAdmin { get; set; }
    }
}
