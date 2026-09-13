using REALESTATE_.API.models;

namespace REALESTATE_.API.DTos
{
    public class PropertyDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string Address { get; set; } = string.Empty;
        public string Neighborhood {  get; set; } = string.Empty;
        public int Bedrooms { get; set; }
        public int Bathrooms { get; set; }
        public double Area { get; set; }
        public PropertyStatus Status { get; set; }
        public int OwnerId { get; set; }
        public string? RejectionReason { get; set; }
        public OwnerDto? Owner { get; set; }
        public List<string> Images { get; set; } = new List<string>();
    }
}
