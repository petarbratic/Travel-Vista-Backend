namespace Explorer.Tours.API.Dtos;

public class TourWishlistDto
{
    public long Id { get; set; }
    public long TouristId { get; set; }
    public long TourId { get; set; }
    public DateTime CreatedAt { get; set; }
    public TourPreviewDto? Tour { get; set; }
}
