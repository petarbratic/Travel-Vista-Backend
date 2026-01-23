namespace Explorer.API.Dtos.Author
{
    public class AuthorProfileStatsDto
    {
        public long AuthorId { get; set; }
        public int TotalTours { get; set; }
        public int TotalReviews { get; set; }
        public double AverageRating { get; set; }
        public int TotalPurchases { get; set; }
        public List<RecentTourReviewDto> RecentReviews { get; set; } = new();
    }
}
