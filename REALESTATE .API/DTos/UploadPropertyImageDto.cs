using System.ComponentModel.DataAnnotations;

namespace REALESTATE_.API.DTos
{
    public class UploadPropertyImageDto
    {
        [Required]
        public IFormFile? Image { get; set; }
    }
}
