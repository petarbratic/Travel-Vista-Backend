using AutoMapper;
using Explorer.BuildingBlocks.Core.Exceptions;
using Explorer.Tours.API.Dtos;
using Explorer.Tours.API.Public.Authoring;
using Explorer.Tours.API.Internal;
using Explorer.Tours.Core.Domain;
using Explorer.Tours.Core.Domain.RepositoryInterfaces;
using System.Linq;

namespace Explorer.Tours.Core.UseCases.Authoring;

public class CouponService : ICouponService
{
    private readonly ICouponRepository _couponRepository;
    private readonly ITourRepository _tourRepository;
    private readonly IInternalTourService _internalTourService;
    private readonly IMapper _mapper;
    private const string Characters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

    public CouponService(ICouponRepository couponRepository, ITourRepository tourRepository, IInternalTourService internalTourService, IMapper mapper)
    {
        _couponRepository = couponRepository;
        _tourRepository = tourRepository;
        _internalTourService = internalTourService;
        _mapper = mapper;
    }

    public CouponDto Create(CouponCreateDto couponDto, long authorId)
    {
        if (authorId == 0)
            throw new ArgumentException("Author ID must be valid.", nameof(authorId));

        // Validacija ture ako je prosleđena
        if (couponDto.TourId.HasValue)
        {
            var tour = _tourRepository.GetById(couponDto.TourId.Value);
            if (tour == null)
                throw new NotFoundException($"Tour with id {couponDto.TourId.Value} not found.");
            if (tour.AuthorId != authorId)
                throw new ForbiddenException("You can only create coupons for your own tours.");
        }

        // Generisanje nasumičnog koda od 8 karaktera
        string code = GenerateRandomCode();

        // Provera da li kod već postoji
        while (_couponRepository.GetByCode(code) != null)
        {
            code = GenerateRandomCode();
        }

        var coupon = new Coupon(code, couponDto.DiscountPercentage, authorId, couponDto.ExpiryDate, couponDto.TourId);
        var result = _couponRepository.Create(coupon);
        return _mapper.Map<CouponDto>(result);
    }

    public CouponDto Update(long id, CouponUpdateDto couponDto, long authorId)
    {
        var coupon = _couponRepository.GetById(id);
        if (coupon == null)
            throw new NotFoundException($"Coupon with id {id} not found.");
        if (coupon.AuthorId != authorId)
            throw new ForbiddenException("You can only update your own coupons.");

        // Validacija ture ako je prosleđena
        if (couponDto.TourId.HasValue)
        {
            var tour = _tourRepository.GetById(couponDto.TourId.Value);
            if (tour == null)
                throw new NotFoundException($"Tour with id {couponDto.TourId.Value} not found.");
            if (tour.AuthorId != authorId)
                throw new ForbiddenException("You can only assign coupons to your own tours.");
        }

        coupon.Update(couponDto.DiscountPercentage, couponDto.ExpiryDate, couponDto.TourId);
        var result = _couponRepository.Update(coupon);
        return _mapper.Map<CouponDto>(result);
    }

    public void Delete(long id, long authorId)
    {
        var coupon = _couponRepository.GetById(id);
        if (coupon == null)
            throw new NotFoundException($"Coupon with id {id} not found.");
        if (coupon.AuthorId != authorId)
            throw new ForbiddenException("You can only delete your own coupons.");

        _couponRepository.Delete(id);
    }

    public CouponDto GetById(long id, long authorId)
    {
        var coupon = _couponRepository.GetById(id);
        if (coupon == null)
            throw new NotFoundException($"Coupon with id {id} not found.");
        if (coupon.AuthorId != authorId)
            throw new ForbiddenException("You can only view your own coupons.");

        return _mapper.Map<CouponDto>(coupon);
    }

    public List<CouponDto> GetByAuthorId(long authorId)
    {
        var coupons = _couponRepository.GetByAuthorId(authorId);
        return coupons.Select(_mapper.Map<CouponDto>).ToList();
    }

    public CouponValidationResultDto ValidateCoupon(string code, long tourId)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            return new CouponValidationResultDto
            {
                IsValid = false,
                Message = "Coupon code cannot be empty.",
                DiscountPercentage = 0,
                DiscountAmount = 0,
                OriginalPrice = 0,
                FinalPrice = 0,
                AppliedToTourId = null
            };
        }

        var coupon = _couponRepository.GetByCode(code);
        if (coupon == null)
        {
            return new CouponValidationResultDto
            {
                IsValid = false,
                Message = "Invalid coupon code.",
                DiscountPercentage = 0,
                DiscountAmount = 0,
                OriginalPrice = 0,
                FinalPrice = 0,
                AppliedToTourId = null
            };
        }

        var tour = _tourRepository.GetById(tourId);
        if (tour == null)
        {
            return new CouponValidationResultDto
            {
                IsValid = false,
                Message = "Tour not found.",
                DiscountPercentage = 0,
                DiscountAmount = 0,
                OriginalPrice = 0,
                FinalPrice = 0,
                AppliedToTourId = null
            };
        }

        if (!coupon.IsValidForTour(tourId, tour.AuthorId))
        {
            string message = !coupon.IsValid() 
                ? "Coupon has expired." 
                : "This coupon is not valid for this tour.";

            // Get discounted price for display
            var discountedPrice = _internalTourService.GetDiscountedPrice(tourId, tour.Price);

            return new CouponValidationResultDto
            {
                IsValid = false,
                Message = message,
                DiscountPercentage = 0,
                DiscountAmount = 0,
                OriginalPrice = discountedPrice,
                FinalPrice = discountedPrice,
                AppliedToTourId = null
            };
        }

        // Get discounted price (applies sale discounts first)
        var tourDiscountedPrice = _internalTourService.GetDiscountedPrice(tourId, tour.Price);
        
        // Apply coupon discount to the already discounted price
        var discountAmount = coupon.CalculateDiscount(tourDiscountedPrice);
        var finalPrice = tourDiscountedPrice - discountAmount;

        return new CouponValidationResultDto
        {
            IsValid = true,
            Message = "Coupon is valid.",
            DiscountPercentage = coupon.DiscountPercentage,
            DiscountAmount = discountAmount,
            OriginalPrice = tourDiscountedPrice,
            FinalPrice = finalPrice,
            AppliedToTourId = tourId
        };
    }

    public CouponValidationResultDto ValidateCouponForCart(string code, List<long> tourIds, Dictionary<long, decimal>? tourPrices = null)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            return new CouponValidationResultDto
            {
                IsValid = false,
                Message = "Coupon code cannot be empty.",
                DiscountPercentage = 0,
                DiscountAmount = 0,
                OriginalPrice = 0,
                FinalPrice = 0,
                AppliedToTourId = null
            };
        }

        if (tourIds == null || tourIds.Count == 0)
        {
            return new CouponValidationResultDto
            {
                IsValid = false,
                Message = "Cart is empty.",
                DiscountPercentage = 0,
                DiscountAmount = 0,
                OriginalPrice = 0,
                FinalPrice = 0,
                AppliedToTourId = null
            };
        }

        var coupon = _couponRepository.GetByCode(code);
        if (coupon == null)
        {
            return new CouponValidationResultDto
            {
                IsValid = false,
                Message = "Invalid coupon code.",
                DiscountPercentage = 0,
                DiscountAmount = 0,
                OriginalPrice = 0,
                FinalPrice = 0,
                AppliedToTourId = null
            };
        }

        // Ako je kupon vezan za određenu turu
        if (coupon.TourId.HasValue)
        {
            var targetTourId = coupon.TourId.Value;
            if (!tourIds.Contains(targetTourId))
            {
                return new CouponValidationResultDto
                {
                    IsValid = false,
                    Message = "This coupon is valid for a specific tour that is not in your cart.",
                    DiscountPercentage = 0,
                    DiscountAmount = 0,
                    OriginalPrice = 0,
                    FinalPrice = 0,
                    AppliedToTourId = null
                };
            }

            // Koristi cenu iz korpe ako je dostupna, inače koristi GetDiscountedPrice
            var tour = _tourRepository.GetById(targetTourId);
            if (tour == null)
            {
                return new CouponValidationResultDto
                {
                    IsValid = false,
                    Message = "Tour not found.",
                    DiscountPercentage = 0,
                    DiscountAmount = 0,
                    OriginalPrice = 0,
                    FinalPrice = 0,
                    AppliedToTourId = null
                };
            }

            decimal tourPrice;
            if (tourPrices != null && tourPrices.ContainsKey(targetTourId))
            {
                tourPrice = tourPrices[targetTourId];
            }
            else
            {
                tourPrice = _internalTourService.GetDiscountedPrice(targetTourId, tour.Price);
            }

            if (!coupon.IsValidForTour(targetTourId, tour.AuthorId))
            {
                string message = !coupon.IsValid() 
                    ? "Coupon has expired." 
                    : "This coupon is not valid for this tour.";

                return new CouponValidationResultDto
                {
                    IsValid = false,
                    Message = message,
                    DiscountPercentage = 0,
                    DiscountAmount = 0,
                    OriginalPrice = tourPrice,
                    FinalPrice = tourPrice,
                    AppliedToTourId = null
                };
            }

            // Apply coupon discount to the price from cart (already discounted)
            var couponDiscountAmount = coupon.CalculateDiscount(tourPrice);
            var couponFinalPrice = tourPrice - couponDiscountAmount;

            return new CouponValidationResultDto
            {
                IsValid = true,
                Message = "Coupon is valid.",
                DiscountPercentage = coupon.DiscountPercentage,
                DiscountAmount = couponDiscountAmount,
                OriginalPrice = tourPrice,
                FinalPrice = couponFinalPrice,
                AppliedToTourId = targetTourId
            };
        }

        // Ako je kupon za sve ture autora, pronađi najskuplji proizvod od tog autora
        var tours = tourIds.Select(id => _tourRepository.GetById(id))
                           .Where(t => t != null)
                           .ToList();

        if (tours.Count == 0)
        {
            return new CouponValidationResultDto
            {
                IsValid = false,
                Message = "No valid tours found in cart.",
                DiscountPercentage = 0,
                DiscountAmount = 0,
                OriginalPrice = 0,
                FinalPrice = 0,
                AppliedToTourId = null
            };
        }

        // Pronađi ture od autora kupona
        var authorTours = tours.Where(t => t.AuthorId == coupon.AuthorId).ToList();
        if (authorTours.Count == 0)
        {
            return new CouponValidationResultDto
            {
                IsValid = false,
                Message = "This coupon is not valid for any tours in your cart.",
                DiscountPercentage = 0,
                DiscountAmount = 0,
                OriginalPrice = 0,
                FinalPrice = 0,
                AppliedToTourId = null
            };
        }

        // Pronađi najskuplji proizvod od autora (koristi cenu iz korpe ako je dostupna)
        var toursWithPrices = authorTours.Select(t => new
        {
            Tour = t,
            Price = tourPrices != null && tourPrices.ContainsKey(t.Id) 
                ? tourPrices[t.Id] 
                : _internalTourService.GetDiscountedPrice(t.Id, t.Price)
        }).OrderByDescending(x => x.Price).ToList();

        var mostExpensiveTourData = toursWithPrices.First();
        var mostExpensiveTour = mostExpensiveTourData.Tour;
        var mostExpensiveTourPrice = mostExpensiveTourData.Price;

        if (!coupon.IsValidForTour(mostExpensiveTour.Id, mostExpensiveTour.AuthorId))
        {
            string message = !coupon.IsValid() 
                ? "Coupon has expired." 
                : "This coupon is not valid for this tour.";

            return new CouponValidationResultDto
            {
                IsValid = false,
                Message = message,
                DiscountPercentage = 0,
                DiscountAmount = 0,
                OriginalPrice = mostExpensiveTourPrice,
                FinalPrice = mostExpensiveTourPrice,
                AppliedToTourId = null
            };
        }

        // Apply coupon discount to the price from cart (already discounted)
        var discountAmount = coupon.CalculateDiscount(mostExpensiveTourPrice);
        var finalPrice = mostExpensiveTourPrice - discountAmount;

        return new CouponValidationResultDto
        {
            IsValid = true,
            Message = "Coupon is valid.",
            DiscountPercentage = coupon.DiscountPercentage,
            DiscountAmount = discountAmount,
            OriginalPrice = mostExpensiveTourPrice,
            FinalPrice = finalPrice,
            AppliedToTourId = mostExpensiveTour.Id
        };
    }

    private string GenerateRandomCode()
    {
        var random = new Random();
        return new string(Enumerable.Repeat(Characters, 8)
            .Select(s => s[random.Next(s.Length)]).ToArray());
    }
}
