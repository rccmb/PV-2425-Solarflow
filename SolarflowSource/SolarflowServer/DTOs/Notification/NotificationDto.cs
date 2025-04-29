using System;

namespace SolarflowServer.DTOs.Notification
{
    /// <summary>
    /// Represents the data transfer object for a notification.
    /// </summary>
    public class NotificationDto
    {
        /// <summary>
        /// Gets or sets the unique identifier of the notification.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the title of the notification.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets the description or content of the notification.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the date and time when the notification was sent.
        /// </summary>
        public DateTime TimeSent { get; set; }

        /// <summary>
        /// Gets or sets the date and time when the notification was read, if applicable.
        /// </summary>
        public DateTime? TimeRead { get; set; }

        /// <summary>
        /// Gets or sets the status of the notification.
        /// </summary>
        /// <remarks>
        /// The status could represent whether the notification is unread, read, or archived.
        /// </remarks>
        public string Status { get; set; }
    }
}