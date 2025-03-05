namespace SolarflowSource.Server.Models
{
    public class ViewAccount
    {
        public int ViewId { get; set; }

        public int UserId { get; set; }

        public string LoginName { get; set; } = string.Empty;

        public string Salt { get; set; } = string.Empty;

        public string HashedPassword { get; set; } = string.Empty;

        public UserAccount UserAccount { get; set; } = null!;
    }
}
