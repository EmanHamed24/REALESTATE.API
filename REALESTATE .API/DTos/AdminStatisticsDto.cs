namespace REALESTATE_.API.DTos
{
    public class AdminStatisticsDto
    {
        public int TotalProperties {  get; set; }
        public int PendingProperties { get; set; }
        public int ApprovedProperties { get; set; }
        public int RejectedProperties { get; set; }
        public int SoldProperties { get; set; }
        public int TotalUsers { get; set; }
        public int OwnerUsers { get; set; }
        public int BuyerUsers { get; set; }
        public int AdminUsers { get; set; }
    }
}
