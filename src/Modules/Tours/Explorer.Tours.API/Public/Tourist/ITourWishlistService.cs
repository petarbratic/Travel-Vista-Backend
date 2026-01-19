using Explorer.Tours.API.Dtos;

namespace Explorer.Tours.API.Public.Tourist;

public interface ITourWishlistService
{
    TourWishlistDto AddToWishlist(long touristId, long tourId);
    void RemoveFromWishlist(long touristId, long tourId);
    List<TourPreviewDto> GetWishlistTours(long touristId);
    bool IsInWishlist(long touristId, long tourId);
}
