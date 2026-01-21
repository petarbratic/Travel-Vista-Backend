using Explorer.Tours.Core.Domain;
using Explorer.Tours.Core.Domain.RepositoryInterfaces;
using Explorer.Tours.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace Explorer.Tours.Infrastructure.Database.Repositories;

public class TourWishlistDbRepository : ITourWishlistRepository
{
    private readonly ToursContext _context;

    public TourWishlistDbRepository(ToursContext context)
    {
        _context = context;
    }

    public TourWishlist Create(TourWishlist wishlist)
    {
        _context.TourWishlists.Add(wishlist);
        _context.SaveChanges();
        return wishlist;
    }

    public void Delete(long id)
    {
        var wishlist = _context.TourWishlists.Find(id);
        if (wishlist != null)
        {
            _context.TourWishlists.Remove(wishlist);
            _context.SaveChanges();
        }
    }

    public TourWishlist? GetById(long id)
    {
        return _context.TourWishlists.Find(id);
    }

    public TourWishlist? GetByTouristAndTour(long touristId, long tourId)
    {
        return _context.TourWishlists
            .FirstOrDefault(w => w.TouristId == touristId && w.TourId == tourId);
    }

    public List<TourWishlist> GetAllForTourist(long touristId)
    {
        return _context.TourWishlists
            .Where(w => w.TouristId == touristId)
            .OrderByDescending(w => w.CreatedAt)
            .ToList();
    }

    public bool IsInWishlist(long touristId, long tourId)
    {
        return _context.TourWishlists
            .Any(w => w.TouristId == touristId && w.TourId == tourId);
    }

    public List<long> GetTouristIdsForTour(long tourId)
    {
        return _context.TourWishlists
            .Where(w => w.TourId == tourId)
            .Select(w => w.TouristId)
            .Distinct()
            .ToList();
    }

}
