namespace SolarflowClient.Models.ViewModels.Authentication
{
    /// <summary>
    /// Represents the data required to confirm a user's email address.
    /// </summary>
    public class ConfirmEmailViewModel
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
