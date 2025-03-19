using System.ComponentModel.DataAnnotations;

namespace SolarflowServer.Models
{
    public class ForgotPasswordRequest
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }
}
