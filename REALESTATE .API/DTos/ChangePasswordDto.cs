using System.ComponentModel.DataAnnotations;

namespace REALESTATE_.API.DTos
{
    public class ChangePasswordDto
    {
        [Required]
        public string CurrentPassword { get; set; } = string.Empty;

        [Required]
        [MinLength(8)]
        [RegularExpression(
            @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).+$",
            ErrorMessage = "Password must contain at least one uppercase letter, one lowercase letter, and one number."
        )]
        public string NewPassword { get; set; } = string.Empty;
    }
}

