using System.Text.Json.Serialization;
namespace REALESTATE_.API.models
{
    public class Property
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string Address { get; set; } = string.Empty;
        public string Neighborhood { get; set; } = string.Empty;
        public int Bedrooms { get; set; }
        public int Bathrooms { get; set; }
        public double Area { get; set; }
        public PropertyStatus Status { get; set; } = PropertyStatus.Pending;
        public string? RejectionReason { get; set; }
        public int OwnerId { get; set; }
        [JsonIgnore]
        public Owner? Owner { get; set; }
        public ICollection<PropertyImage> Images { get; set; } = new List<PropertyImage>();
    }
}
