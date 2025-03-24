namespace SolarflowClient.Models.ViewModels.Authentication
{
    public class GetUserViewModel
    {
        public string Fullname { get; set; }
        public string Photo { get; set; }
        public string Email { get; set; }
        public DateTime CreatedAt { get; set; }

        public bool HasViewAccount { get; set; }
    }
}
