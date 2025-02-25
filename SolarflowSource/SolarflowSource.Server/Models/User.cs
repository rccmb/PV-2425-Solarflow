namespace SolarflowSource.Server.Models
{
    public class User
    {
        private static int ID_COUNTER;

        public int Id { get; set; }

        public string Username { get; set; }

        // TODO Photo.

        public string Email { get; set; }

        // TODO Remove mock password. SALT, HASHED.
        public string Password { get; set; }

        public bool ViewAccountStatus { get; set; }

        // TODO Review.
        public string BatteryAPI { get; set; }

        public DateTime CreatedAt { get; set; }

        public User(string username, string email, string password, bool viewAccountStatus, string batteryAPI)
        {   
            Id = ID_COUNTER;
            ID_COUNTER++;

            Username = username;

            Email = email;

            Password = password;

            ViewAccountStatus = viewAccountStatus;

            BatteryAPI = batteryAPI;

            CreatedAt = DateTime.UtcNow; 
        }
    }
}
