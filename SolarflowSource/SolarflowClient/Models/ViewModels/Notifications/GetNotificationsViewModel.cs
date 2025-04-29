using System;

namespace SolarflowClient.Models.ViewModels.Notifications
{
    /// <summary>
    /// Represents the data required to display a notification to the user.
    /// </summary>
    public class GetNotificationsViewModel
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
        /// Gets or sets the description or details of the notification.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the status of the notification (e.g., Unread or Read).
        /// </summary>
        public string Status { get; set; }

        /// <summary>
        /// Gets or sets the timestamp of when the notification was sent.
        /// </summary>
        public DateTime TimeSent { get; set; }

        /// <summary>
        /// Gets or sets the timestamp of when the notification was read, if applicable.
        /// </summary>
        public DateTime? TimeRead { get; set; }
    }
}