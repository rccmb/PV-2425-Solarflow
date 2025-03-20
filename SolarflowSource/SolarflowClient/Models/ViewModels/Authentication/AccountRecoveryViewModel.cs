using System.ComponentModel.DataAnnotations;

namespace SolarflowClient.Models.ViewModels.Authentication
{
    public class AccountRecoveryViewModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        
        public string? ClientUri { get; set; }
    }

}
