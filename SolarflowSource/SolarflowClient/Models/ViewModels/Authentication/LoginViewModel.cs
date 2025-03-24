using System.ComponentModel.DataAnnotations;

namespace SolarflowClient.Models.ViewModels.Authentication
{
    public class LoginViewModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        public bool RememberMe { get; set; } = false;

        public bool IsAdmin { get; set; } = false;
    }
}
