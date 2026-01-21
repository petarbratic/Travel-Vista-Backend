using AutoMapper;
using Explorer.BuildingBlocks.Core.UseCases;
using Explorer.Stakeholders.API.Dtos;
using Explorer.Stakeholders.API.Public;
using Explorer.Stakeholders.Core.Domain;
using Explorer.Stakeholders.Core.Domain.RepositoryInterfaces;

namespace Explorer.Stakeholders.Core.UseCases
{
    public class AppRatingService : IAppRatingService
    {
        private readonly IAppRatingRepository _repository;
        private readonly IUserRepository _userRepository;
        private readonly IPersonRepository _personRepository; // DODATO
        private readonly ITouristRepository _touristRepository;
        private readonly IFirstTimeXpService _firstTimeXpService;
        private readonly IMapper _mapper;

        public AppRatingService(
            IAppRatingRepository repository,
            IUserRepository userRepository,
            IPersonRepository personRepository,
            ITouristRepository touristRepository,
            IFirstTimeXpService firstTimeXpService,
            IMapper mapper)
        {
            _repository = repository;
            _userRepository = userRepository;
            _personRepository = personRepository;
            _touristRepository = touristRepository;
            _firstTimeXpService = firstTimeXpService;
            _mapper = mapper;
        }

        public AppRatingResponseDto CreateRating(long userId, AppRatingRequestDto entity)
        {
            var rating = _repository.GetByUserId(userId);

            if (rating == null) // ← Prvi put
            {
                var newRating = new AppRating(userId, entity.Rating, entity.Comment);
                _repository.Create(newRating);
                rating = newRating;

                // Award XP - koristi rating.Id kao SourceEntityId
                var person = _personRepository.GetByUserId(userId);
                if (person != null)
                {
                    var tourist = _touristRepository.GetByPersonId(person.Id);
                    if (tourist != null)
                    {
                        _firstTimeXpService.TryAwardFirstAppReview(tourist.Id, rating.Id); // ← rating.Id je unique!
                    }
                }
            }
            else // ← Update postojećeg
            {
                // Ne daje XP za update
                _mapper.Map(entity, rating);
                rating.Validate();
                rating.UpdatedAt = DateTime.Now;
                _repository.Update(rating);
            }

            return MapToDto(rating);
        }

        public AppRatingResponseDto UpdateRating(long userId, AppRatingRequestDto entity)
        {
            var rating = _repository.GetByUserId(userId);
            if (rating == null)
            {
                throw new KeyNotFoundException($"Rating for user {userId} not found. Cannot update non-existing rating.");
            }

            _mapper.Map(entity, rating);
            rating.Validate();
            rating.UpdatedAt = DateTime.Now;
            _repository.Update(rating);

            return MapToDto(rating);
        }

        public void DeleteRating(long userId)
        {
            var rating = _repository.GetByUserId(userId);
            if (rating == null)
            {
                throw new KeyNotFoundException($"Rating for user {userId} not found.");
            }

            _repository.Delete(rating);
        }

        public AppRatingResponseDto? GetMyRating(long userId)
        {
            var rating = _repository.GetByUserId(userId);
            return rating == null ? null : MapToDto(rating);
        }

        public PagedResult<AppRatingResponseDto> GetPaged(int page, int pageSize)
        {
            var result = _repository.GetPaged(page, pageSize);
            var items = result.Results.Select(MapToDto).ToList();
            return new PagedResult<AppRatingResponseDto>(items, result.TotalCount);
        }

        private AppRatingResponseDto MapToDto(AppRating entity)
        {
            var rating = _mapper.Map<AppRatingResponseDto>(entity);
            var user = _userRepository.Get(entity.UserId);
            rating.Username = user?.Username ?? string.Empty;
            return rating;
        }
    }
}