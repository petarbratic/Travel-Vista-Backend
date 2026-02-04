using Explorer.BuildingBlocks.Core.Domain;

namespace Explorer.Tours.Core.Domain;

public class TourWishlist : Entity
{
    public long TouristId { get; private set; }
    public long TourId { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private TourWishlist() { }

    public TourWishlist(long touristId, long tourId)
    {
        if (touristId == 0)
            throw new ArgumentException("Tourist ID must be valid (cannot be 0).", nameof(touristId));
        if (tourId == 0)
            throw new ArgumentException("Tour ID must be valid (cannot be 0).", nameof(tourId));

        TouristId = touristId;
        TourId = tourId;
        CreatedAt = DateTime.UtcNow;
    }
}
