using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SolarflowServer.Models
{
    public class Notification
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(150)]
        public string Title { get; set; }

        [Required]
        [MaxLength(500)]
        public string Description { get; set; }

        public DateTime TimeSent { get; set; } = DateTime.UtcNow;

        public DateTime? TimeRead { get; set; }

        public NotificationStatus Status { get; set; } = NotificationStatus.Unread;

        [Required]
        public int UserId { get; set; }

        [ForeignKey("UserId")]
        public ApplicationUser User { get; set; }

        
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