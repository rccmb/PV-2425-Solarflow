namespace SolarflowSource.Server.Models
{
    public class User
    {
        public int Id { get; set; }

        public string Username { get; set; }

        // TODO: Photo.

        public string Email { get; set; }

        // TODO: Remove mock password. SALT, HASHED.
        public string HashedPassword { get; set; }

        public bool ViewAccountStatus { get; set; }

        // TODO: Review.
        public string BatteryAPI { get; set; }

        public DateTime CreatedAt { get; set; }

        public User() { }

        public User(int id, string username, string email, string hashedPassword, bool viewAccountStatus, string batteryAPI)
        {
            Id = id;

            Username = username;

            // TODO: Photo

            Email = email;

            HashedPassword = hashedPassword;

            ViewAccountStatus = viewAccountStatus;

            BatteryAPI = batteryAPI;

            CreatedAt = DateTime.UtcNow;
        }
    }
}
