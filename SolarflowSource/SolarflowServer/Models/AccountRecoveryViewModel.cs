using System.ComponentModel.DataAnnotations;

namespace SolarflowServer.Models
{
    public class AccountRecoveryViewModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }
}
