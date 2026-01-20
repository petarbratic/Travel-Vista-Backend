using AutoMapper;
using Explorer.BuildingBlocks.Core.Exceptions;
using Explorer.Tours.API.Dtos;
using Explorer.Tours.API.Internal;
using Explorer.Tours.API.Public.Authoring;
using Explorer.Tours.Core.Domain;
using Explorer.Tours.Core.Domain.RepositoryInterfaces;

namespace Explorer.Tours.Core.UseCases.Authoring;

public class SaleService : ISaleService
{
    private readonly ISaleRepository _saleRepository;
    private readonly ITourRepository _tourRepository;
    private readonly ITourWishlistRepository _wishlistRepository;
    private readonly IInternalNotificationService _notificationService;
    private readonly INotificationRepository _notificationRepository; 
    private readonly IMapper _mapper;

    public SaleService(
        ISaleRepository saleRepository,
        ITourRepository tourRepository,
        ITourWishlistRepository wishlistRepository,
        IInternalNotificationService notificationService,
        INotificationRepository notificationRepository,
        IMapper mapper)
    {
        _saleRepository = saleRepository;
        _tourRepository = tourRepository;
        _wishlistRepository = wishlistRepository;
        _notificationService = notificationService;
        _notificationRepository = notificationRepository;
        _mapper = mapper;
    }

    public SaleDto Create(SaleCreateDto saleDto, long authorId)
    {
        // Validate that all tours exist and belong to the author
        foreach (var tourId in saleDto.TourIds)
        {
            var tour = _tourRepository.GetById(tourId);
            if (tour == null)
                throw new NotFoundException($"Tour with id {tourId} not found.");
            if (tour.AuthorId != authorId)
                throw new ForbiddenException($"Tour {tourId} does not belong to you.");
        }

        var sale = new Sale(
            saleDto.TourIds,
            saleDto.StartDate,
            saleDto.EndDate,
            saleDto.DiscountPercentage,
            authorId
        );

        var result = _saleRepository.Create(sale);

        if (IsActiveNow(result.StartDate, result.EndDate))
        {
            NotifyWishlistersForTours(result.TourIds, result.DiscountPercentage);
        }

        return _mapper.Map<SaleDto>(result);
    }

    public SaleDto Update(SaleUpdateDto saleDto, long authorId)
    {
        var sale = _saleRepository.GetById(saleDto.Id);
        if (sale == null)
            throw new NotFoundException($"Sale with id {saleDto.Id} not found.");
        if (sale.AuthorId != authorId)
            throw new ForbiddenException("You can only update your own sales.");

        var wasActive = IsActiveNow(sale.StartDate, sale.EndDate);
        var oldTourIds = sale.TourIds.ToList();

        // Validate that all tours exist and belong to the author
        foreach (var tourId in saleDto.TourIds)
        {
            var tour = _tourRepository.GetById(tourId);
            if (tour == null)
                throw new NotFoundException($"Tour with id {tourId} not found.");
            if (tour.AuthorId != authorId)
                throw new ForbiddenException($"Tour {tourId} does not belong to you.");
        }

        sale.Update(saleDto.TourIds, saleDto.StartDate, saleDto.EndDate, saleDto.DiscountPercentage);

        var result = _saleRepository.Update(sale);

        var isActive = IsActiveNow(result.StartDate, result.EndDate);

        if (!wasActive && isActive)
        {
            // sale je upravo postao aktivan, šalji za sve ture u njemu
            NotifyWishlistersForTours(result.TourIds, result.DiscountPercentage);
        }
        else if (wasActive && isActive)
        {
            // sale je i dalje aktivan, ali možda su dodate nove ture
            var added = result.TourIds.Except(oldTourIds).ToList();
            if (added.Any())
            {
                NotifyWishlistersForTours(added, result.DiscountPercentage);
            }
        }

        return _mapper.Map<SaleDto>(result);
    }

    public void Delete(long id, long authorId)
    {
        var sale = _saleRepository.GetById(id);
        if (sale == null)
            throw new NotFoundException($"Sale with id {id} not found.");
        if (sale.AuthorId != authorId)
            throw new ForbiddenException("You can only delete your own sales.");

        _saleRepository.Delete(id);
    }

    public SaleDto GetById(long id)
    {
        var sale = _saleRepository.GetById(id);
        if (sale == null)
            throw new NotFoundException($"Sale with id {id} not found.");

        return _mapper.Map<SaleDto>(sale);
    }

    public List<SaleDto> GetByAuthorId(long authorId)
    {
        var sales = _saleRepository.GetByAuthorId(authorId);
        return sales.Select(_mapper.Map<SaleDto>).ToList();
    }

    private static bool IsActiveNow(DateTime start, DateTime end)
    {
        var now = DateTime.UtcNow;
        return start <= now && end >= now;
    }

    private void NotifyWishlistersForTours(IEnumerable<long> tourIds, decimal discountPercentage)
    {
        foreach (var tourId in tourIds.Distinct())
        {
            var tour = _tourRepository.GetById(tourId);
            if (tour == null) continue;

            var touristIds = _wishlistRepository.GetTouristIdsForTour(tourId);

            foreach (var touristId in touristIds)
            {
                if (_notificationRepository.Exists(touristId, NotificationType.TourOnSale, tourId))
                    continue;

                _notificationService.CreateTourOnSaleNotification(
                    recipientId: touristId,
                    tourId: tourId,
                    tourName: tour.Name,
                    discountPercentage: discountPercentage
                );
            }
        }
    }

}
