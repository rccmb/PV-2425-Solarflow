using System.ComponentModel.DataAnnotations;

namespace SolarflowServer.DTOs.Authentication
{
    public class ResetPasswordDTO
    {
        [Required(ErrorMessage = "Passwoed is required")]
        [DataType(DataType.Password)]
        public string? Password { get; set; }

        [Compare("Password", ErrorMessage = "Passwords do not match")]
        public string? ConfirmPassword { get; set; }

        public string? Token { get; set; }
        public string? Email { get; set; }
    }
}
