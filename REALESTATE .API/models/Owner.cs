namespace REALESTATE_.API.models
{
    public class Owner
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string Password { get; set; } = string.Empty;
        public string Role { get; set; } = "Owner";

        public ICollection<Property> Properties { get; set; } = new List<Property>();
    }
}
