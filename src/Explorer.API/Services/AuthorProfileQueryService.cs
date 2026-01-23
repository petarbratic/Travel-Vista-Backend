using Explorer.API.Dtos.Author;
using Explorer.Payments.Core.Domain.RepositoryInterfaces;
using Explorer.Stakeholders.Core.Domain.RepositoryInterfaces;
using Explorer.Tours.Core.Domain.RepositoryInterfaces;
namespace Explorer.API.Services
{
    public class AuthorProfileQueryService : IAuthorProfileQueryService
    {
        private readonly IPersonRepository _personRepository;
        private readonly ITourRepository _tourRepository;
        private readonly ITourReviewRepository _tourReviewRepository;
        private readonly ITourPurchaseTokenRepository _tourPurchaseTokenRepository;

        public AuthorProfileQueryService(
            IPersonRepository personRepository,
            ITourRepository tourRepository,
            ITourReviewRepository tourReviewRepository,
            ITourPurchaseTokenRepository tourPurchaseTokenRepository)
        {
            _personRepository = personRepository;
            _tourRepository = tourRepository;
            _tourReviewRepository = tourReviewRepository;
            _tourPurchaseTokenRepository = tourPurchaseTokenRepository;
        }

        public AuthorProfileStatsDto GetMyStats(long currentUserId)
        {
            // 1) Person (da profil postoji)
            var person = _personRepository.GetByUserId(currentUserId);
            if (person == null)
                throw new KeyNotFoundException("Person not found for current user.");

            // 2) Moje ture
            var myTours = _tourRepository.GetByAuthorId(currentUserId);
            var tourIds = myTours.Select(t => t.Id).ToList();

            if (!tourIds.Any())
            {
                return new AuthorProfileStatsDto
                {
                    AuthorId = currentUserId,
                    TotalTours = 0,
                    TotalReviews = 0,
                    AverageRating = 0,
                    TotalPurchases = 0,
                    RecentReviews = new()
                };
            }

            var tourNameById = myTours.ToDictionary(t => t.Id, t => t.Name);

            // 3) Recenzije (za sada po turi)
            var allReviews = new List<Explorer.Tours.Core.Domain.TourReview>();
            foreach (var id in tourIds)
            {
                allReviews.AddRange(_tourReviewRepository.GetAllForTour(id));
            }

            int totalReviews = allReviews.Count;
            double avgRating = totalReviews > 0 ? allReviews.Average(r => r.Rating) : 0;

            var recent = allReviews
                .OrderByDescending(r => r.CreatedAt)
                .Take(20)
                .Select(r => new RecentTourReviewDto
                {
                    ReviewId = r.Id,
                    TourId = r.TourId,
                    TourName = tourNameById.TryGetValue(r.TourId, out var name) ? name : "Unknown",
                    Rating = r.Rating,
                    Comment = r.Comment,
                    CreatedAt = r.CreatedAt
                })
                .ToList();

            // 4) Broj kupljenih tura (prebroj tokena za moje tourIds)
            int totalPurchases = _tourPurchaseTokenRepository.CountByTourIds(tourIds);

            return new AuthorProfileStatsDto
            {
                AuthorId = currentUserId,
                TotalTours = myTours.Count,
                TotalReviews = totalReviews,
                AverageRating = Math.Round(avgRating, 2),
                TotalPurchases = totalPurchases,
                RecentReviews = recent
            };
        }
    }
}
