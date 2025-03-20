using System.ComponentModel.DataAnnotations;

namespace SolarflowServer.DTOs.Authentication
{
    public class ForgotPasswordDTO
    {
        [Required]
        [EmailAddress]
        public string? Email { get; set; }

        [Required]
        public string? ClientUri { get; set; }
    }
}
