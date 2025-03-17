namespace SolarflowServer.Models
{
    public class AuditLog
    {
        public int Id { get; set; }

        public string UserId { get; set; }

        public string Action { get; set; } 

        public string Email { get; set; }

        public string IPAddress { get; set; }

        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }

}
