using System.ComponentModel.DataAnnotations;

namespace REALESTATE_.API.DTos
{
    public class CreatePropertyDto
    {
        [Required]
        [StringLength(100, MinimumLength = 3)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [StringLength(1000, MinimumLength = 10)]
        public string Description { get; set; } = string.Empty;

        [Range(1, double.MaxValue)]
        public decimal Price { get; set; }

        [Required]
        [StringLength(200)]
        public string Address { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string Neighborhood { get; set; } = string.Empty;

        [Range(2, 8)]
        public int Bedrooms { get; set; }

        [Range(1, 6)]
        public int Bathrooms { get; set; }

        [Range(50, 800)]
        public double Area { get; set; }
    }
}