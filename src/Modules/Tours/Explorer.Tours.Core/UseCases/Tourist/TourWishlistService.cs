using AutoMapper;
using Explorer.BuildingBlocks.Core.Exceptions;
using Explorer.Payments.API.Internal;
using Explorer.Tours.API.Dtos;
using Explorer.Tours.API.Public.Tourist;
using Explorer.Tours.Core.Domain;
using Explorer.Tours.Core.Domain.RepositoryInterfaces;

namespace Explorer.Tours.Core.UseCases.Tourist;

public class TourWishlistService : ITourWishlistService
{
    private readonly ITourWishlistRepository _wishlistRepository;
    private readonly ITourRepository _tourRepository;
    private readonly ITourReviewRepository _reviewRepository;
    private readonly ISaleRepository _saleRepository;
    private readonly ITourAccessService _accessService;
    private readonly IMapper _mapper;

    public TourWishlistService(
        ITourWishlistRepository wishlistRepository,
        ITourRepository tourRepository,
        ITourReviewRepository reviewRepository,
        ISaleRepository saleRepository,
        ITourAccessService accessService,
        IMapper mapper)
    {
        _wishlistRepository = wishlistRepository;
        _tourRepository = tourRepository;
        _reviewRepository = reviewRepository;
        _saleRepository = saleRepository;
        _accessService = accessService;
        _mapper = mapper;
    }

    public TourWishlistDto AddToWishlist(long touristId, long tourId)
    {
        // Check if tour exists
        var tour = _tourRepository.GetById(tourId);
        if (tour == null)
            throw new NotFoundException("Tour not found.");

        // Check if tour is published
        if (tour.Status != TourStatus.Published)
            throw new InvalidOperationException("Only published tours can be added to wishlist.");

        // Check if already in wishlist
        if (_wishlistRepository.IsInWishlist(touristId, tourId))
            throw new InvalidOperationException("Tour is already in wishlist.");

        // Check if already purchased - don't allow adding purchased tours to wishlist
        if (_accessService.HasUserPurchased(touristId, tourId))
            throw new InvalidOperationException("Cannot add purchased tour to wishlist.");

        var wishlist = new TourWishlist(touristId, tourId);
        var created = _wishlistRepository.Create(wishlist);

        return _mapper.Map<TourWishlistDto>(created);
    }

    public void RemoveFromWishlist(long touristId, long tourId)
    {
        var wishlist = _wishlistRepository.GetByTouristAndTour(touristId, tourId);
        if (wishlist == null)
            throw new NotFoundException("Tour is not in wishlist.");

        _wishlistRepository.Delete(wishlist.Id);
    }

    public List<TourPreviewDto> GetWishlistTours(long touristId)
    {
        var wishlistItems = _wishlistRepository.GetAllForTourist(touristId);
        if (!wishlistItems.Any())
            return new List<TourPreviewDto>();

        var tourIds = wishlistItems.Select(w => w.TourId).ToList();
        var tours = tourIds
            .Select(id => _tourRepository.GetByIdWithKeyPoints(id))
            .Where(t => t != null && t.Status == TourStatus.Published)
            .ToList();

        var activeSales = _saleRepository.GetActiveSalesForTours(tourIds);
        var result = new List<TourPreviewDto>();

        foreach (var tour in tours)
        {
            var dto = _mapper.Map<TourPreviewDto>(tour);

            dto.Length = tour.DistanceInKm;

            if (tour.TourDurations != null && tour.TourDurations.Any())
                dto.AverageDuration = tour.TourDurations.Average(td => td.TimeInMinutes);
            else
                dto.AverageDuration = 0;

            if (tour.KeyPoints != null && tour.KeyPoints.Any())
            {
                var first = tour.KeyPoints.OrderBy(k => k.Id).First();
                dto.StartPoint = first.Name;
                dto.FirstKeyPoint = _mapper.Map<KeyPointDto>(first);
            }

            var reviews = _reviewRepository.GetAllForTour(tour.Id);
            if (reviews.Any())
            {
                dto.AverageRating = reviews.Average(r => r.Rating);
                dto.Reviews = reviews.Select(r => _mapper.Map<TourReviewDto>(r)).ToList();
            }
            else
            {
                dto.AverageRating = 0;
                dto.Reviews = new List<TourReviewDto>();
            }

            // Apply sale price
            var sale = activeSales.FirstOrDefault(s => s.TourIds != null && s.TourIds.Contains(tour.Id));
            if (sale != null)
            {
                dto.OnSale = true;
                dto.OriginalPrice = (double)tour.Price;
                dto.DiscountPercentage = (double)sale.DiscountPercentage;
                dto.DiscountedPrice = (double)(tour.Price * (1 - sale.DiscountPercentage / 100));
                dto.Price = dto.DiscountedPrice;
            }
            else
            {
                dto.OnSale = false;
                dto.OriginalPrice = (double)tour.Price;
                dto.DiscountedPrice = (double)tour.Price;
                dto.DiscountPercentage = 0;
                dto.Price = (double)tour.Price;
            }

            result.Add(dto);
        }

        return result;
    }

    public bool IsInWishlist(long touristId, long tourId)
    {
        return _wishlistRepository.IsInWishlist(touristId, tourId);
    }
}
