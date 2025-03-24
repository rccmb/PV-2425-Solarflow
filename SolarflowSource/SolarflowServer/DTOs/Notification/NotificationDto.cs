using System;

namespace SolarflowServer.DTOs.Notification
{
    public class NotificationDto
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime TimeSent { get; set; }
        public DateTime? TimeRead { get; set; }
        public string Status { get; set; }
    }
}