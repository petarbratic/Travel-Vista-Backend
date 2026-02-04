namespace Explorer.API.Dtos.Author
{
    public class RecentTourReviewDto
    {
        public long ReviewId { get; set; }
        public long TourId { get; set; }
        public string TourName { get; set; } = "";
        public int Rating { get; set; }
        public string? Comment { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
