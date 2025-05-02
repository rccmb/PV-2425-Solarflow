namespace SolarflowServer.DTOs.Authentication
{
    /// <summary>
    /// Represents the data transfer object used to confirm a user's email address.
    /// </summary>
    public class ConfirmEmailDTO
    {
        /// <summary>
        /// Gets or sets the unique identifier of the user whose email is being confirmed.
        /// </summary>
        public string UserId { get; set; }

        /// <summary>
        /// Gets or sets the token used to confirm the user's email address.
        /// </summary>
        public string Token { get; set; }
    }
}
