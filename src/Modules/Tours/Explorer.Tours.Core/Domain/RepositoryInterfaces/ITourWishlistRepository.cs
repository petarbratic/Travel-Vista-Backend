namespace Explorer.Tours.Core.Domain.RepositoryInterfaces;

public interface ITourWishlistRepository
{
    TourWishlist Create(TourWishlist wishlist);
    void Delete(long id);
    TourWishlist? GetById(long id);
    TourWishlist? GetByTouristAndTour(long touristId, long tourId);
    List<TourWishlist> GetAllForTourist(long touristId);
    bool IsInWishlist(long touristId, long tourId);
}
