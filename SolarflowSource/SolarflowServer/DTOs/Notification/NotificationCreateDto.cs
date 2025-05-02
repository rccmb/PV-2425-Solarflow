namespace SolarflowServer.DTOs.Notification
{
    /// <summary>
    /// Represents the data transfer object used to create a new notification.
    /// </summary>
    public class NotificationCreateDto
    {
        /// <summary>
        /// Gets or sets the title of the notification.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets the description or content of the notification.
        /// </summary>
        public string Description { get; set; }
    }
}