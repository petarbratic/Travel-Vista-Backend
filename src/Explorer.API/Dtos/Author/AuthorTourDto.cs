namespace Explorer.API.Dtos.Author
{
    public class AuthorTourDto
    {
        public long TourId { get; set; }
        public string Name { get; set; } = "";
        public string? Description { get; set; }
        public int? DurationMinutes { get; set; }
        public double? Price { get; set; }
        public string? Difficulty { get; set; }   
        public string? Status { get; set; }       
        public DateTime? CreatedAt { get; set; }
    }
}
