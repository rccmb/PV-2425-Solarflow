namespace SolarflowSource.Server.Models
{
    public class UserAccount
    {
        public int UserId { get; set; }

        public string Name { get; set; } 

        public string? Photo { get; set; }

        public string Email { get; set; } 

        public string Salt { get; set; } 

        public string HashedPassword { get; set; } 

        public bool ViewAccountStatus { get; set; }

        public bool ConfirmedEmail { get; set; }

        public string? BatteryAPI { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
