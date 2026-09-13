using System.ComponentModel.DataAnnotations;

namespace REALESTATE_.API.DTos
{
    public class CreatePropertyImageDto
    {
        [Required]
        [Url]
        public string ImageUrl { get; set; } = string.Empty;
    }
}
