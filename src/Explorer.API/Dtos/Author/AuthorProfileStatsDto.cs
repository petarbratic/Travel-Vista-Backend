namespace Explorer.API.Dtos.Author
{
    public class AuthorProfileStatsDto
    {
        public long AuthorId { get; set; }
        public int TotalTours { get; set; }
        public int TotalReviews { get; set; }
        public double AverageRating { get; set; }
        public int TotalPurchases { get; set; }
        public string AuthorName { get; set; } = "";
        public string AuthorSurname { get; set; } = "";
        public List<RecentTourReviewDto> RecentReviews { get; set; } = new();
        public List<AuthorTourDto> Tours { get; set; } = new();
    }
}
