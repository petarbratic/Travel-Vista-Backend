namespace Explorer.API.Dtos.Author
{
    public class AuthorTopListItemDto
    {
        public long AuthorId { get; set; }
        public string AuthorName { get; set; } = "";
        public double AverageRating { get; set; }
        public int TotalReviews { get; set; }
        public int TotalTours { get; set; }
        public int TotalPurchases { get; set; }
    }
}
