using System.ComponentModel.DataAnnotations;

namespace REALESTATE_.API.DTos
{
    public class RegisterDto
    {
        [Required]
        [StringLength(100, MinimumLength = 3)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [StringLength(150)]
        public string Email { get; set; } = string.Empty;

        [Required]
        [StringLength(100, MinimumLength = 6)]
        public string Password { get; set; } = string.Empty;

        [Required]
        [RegularExpression(
            "^(Buyer|Owner)$",
            ErrorMessage = "Role must be either Buyer or Owner.")]
        public string Role { get; set; } = "Buyer";
    }
}