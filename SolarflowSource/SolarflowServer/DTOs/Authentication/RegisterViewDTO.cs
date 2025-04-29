namespace SolarflowServer.DTOs.Authentication
{
    /// <summary>
    /// Represents the data transfer object used for registering a user in the view layer.
    /// </summary>
    public class RegisterViewDTO
{
        /// <summary>
        /// Gets or sets the unique identifier of the user.
        /// </summary>
        public int UserId { get; set; }

        /// <summary>
        /// Gets or sets the password for the user's account.
        /// </summary>
        public string Password { get; set; } 
}
}