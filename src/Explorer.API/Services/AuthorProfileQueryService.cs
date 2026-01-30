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
            => BuildStatsForAuthor(currentUserId);

        public AuthorProfileStatsDto GetAuthorStats(long authorId)
            => BuildStatsForAuthor(authorId);

        private AuthorProfileStatsDto BuildStatsForAuthor(long authorId)
        {
            var person = _personRepository.GetByUserId(authorId);
            if (person == null)
                throw new KeyNotFoundException("Author not found.");

            var myTours = _tourRepository.GetByAuthorId(authorId);
            var tourIds = myTours.Select(t => t.Id).ToList();

            if (!tourIds.Any())
            {
                return new AuthorProfileStatsDto
                {
                    AuthorId = authorId,
                    TotalTours = 0,
                    TotalReviews = 0,
                    AverageRating = 0,
                    TotalPurchases = 0,
                    RecentReviews = new()
                };
            }

            var tourNameById = myTours.ToDictionary(t => t.Id, t => t.Name);

            var allReviews = new List<Explorer.Tours.Core.Domain.TourReview>();
            foreach (var id in tourIds)
                allReviews.AddRange(_tourReviewRepository.GetAllForTour(id));

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

            int totalPurchases = _tourPurchaseTokenRepository.CountByTourIds(tourIds);

            return new AuthorProfileStatsDto
            {
                AuthorId = authorId,
                TotalTours = myTours.Count,
                TotalReviews = totalReviews,
                AverageRating = Math.Round(avgRating, 2),
                TotalPurchases = totalPurchases,
                RecentReviews = recent
            };
        }

        public List<AuthorTopListItemDto> GetTopAuthors(string sort, int take)
        {
            var persons = _personRepository.GetAll();
            var items = new List<AuthorTopListItemDto>();

            foreach (var person in persons)
            {
                // autor = ima barem jednu turu
                var tours = _tourRepository.GetByAuthorId(person.UserId);
                if (!tours.Any()) continue;

                var stats = BuildStatsForAuthor(person.UserId);

                items.Add(new AuthorTopListItemDto
                {
                    AuthorId = person.UserId,
                    AuthorName = $"{person.Name} {person.Surname}",
                    AverageRating = stats.AverageRating,
                    TotalReviews = stats.TotalReviews,
                    TotalTours = stats.TotalTours,
                    TotalPurchases = stats.TotalPurchases
                });
            }

            var sortKey = (sort ?? "").ToLowerInvariant();

            // NOVO: top 3 po recenzijama, pa među njima po kupovinama
            if (sortKey == "top3reviewsbypurchases" || sortKey == "reviews3purchases")
            {
                return items
                    .OrderByDescending(x => x.TotalReviews)          // 1) top po recenzijama
                    .ThenByDescending(x => x.TotalPurchases)        // tie-breaker već ovde (nije obavezno)
                    .Take(3)                                        // uzmi prva 3
                    .OrderByDescending(x => x.TotalPurchases)        // 2) sortiraj ta 3 po kupovinama
                    .ThenByDescending(x => x.TotalReviews)           // tie-breaker da bude stabilno
                    .ToList();
            }

            // postojeći sortovi
            items = sortKey switch
            {
                "purchases" => items.OrderByDescending(x => x.TotalPurchases).ToList(),
                "reviews" => items.OrderByDescending(x => x.TotalReviews).ToList(),
                "tours" => items.OrderByDescending(x => x.TotalTours).ToList(),
                _ => items
                    .OrderByDescending(x => x.AverageRating)
                    .ThenByDescending(x => x.TotalReviews)
                    .ToList()
            };

            return items.Take(Math.Clamp(take, 1, 100)).ToList();
        }


    }
}
