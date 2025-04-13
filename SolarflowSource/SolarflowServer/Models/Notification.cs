using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SolarflowServer.Models.Enums;

namespace SolarflowServer.Models
{
    /// <summary>
    /// Represents a notification sent to a user.
    /// </summary>
    public class Notification
    {
        /// <summary>
        /// Gets or sets the unique identifier for the notification.
        /// </summary>
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the title of the notification.
        /// </summary>
        [Required]
        [MaxLength(150)]
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets the description or body of the notification.
        /// </summary>
        [Required]
        [MaxLength(500)]
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the timestamp of when the notification was sent.
        /// </summary>
        public DateTime TimeSent { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Gets or sets the timestamp of when the notification was read, if applicable.
        /// </summary>
        public DateTime? TimeRead { get; set; }

        /// <summary>
        /// Gets or sets the status of the notification (e.g., Unread or Read).
        /// </summary>
        public NotificationStatus Status { get; set; } = NotificationStatus.Unread;

        /// <summary>
        /// Gets or sets the unique identifier of the user associated with the notification.
        /// </summary>
        [Required]
        public int UserId { get; set; }

        /// <summary>
        /// Gets or sets the user associated with the notification.
        /// </summary>
        [ForeignKey("UserId")]
        public ApplicationUser User { get; set; }

        /// <summary>
        /// Marks the notification as read by updating its status and timestamp.
        /// </summary>
        public void MarkAsRead()
        {
            if (Status == NotificationStatus.Unread)
            {
                Status = NotificationStatus.Read;
                TimeRead = DateTime.UtcNow;
            }
        }
    }
}