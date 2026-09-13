using REALESTATE_.API.models;

namespace REALESTATE_.API.models
{
    public class Favorite
    {
        public int Id { get; set; }
        public int BuyerId { get; set; }
        public Owner? Buyer { get; set; }
        public int PropertyId { get; set; }
        public Property? Property { get; set; }
    }
}
