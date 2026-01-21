using AutoMapper;
using Explorer.Stakeholders.API.Dtos;
using Explorer.Stakeholders.API.Public;
using Explorer.Stakeholders.Core.Domain;
using Explorer.Stakeholders.Core.Domain.RepositoryInterfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Explorer.Stakeholders.Core.UseCases
{
    public class AchievementService : IAchievementService
    {
        private readonly IAchievementRepository _achievementRepository;
        private readonly IMapper _mapper;

        public AchievementService(IAchievementRepository achievementRepository, IMapper mapper)
        {
            _achievementRepository = achievementRepository;
            _mapper = mapper;
        }

        public AchievementDto Create(AchievementDto achievementDto, long touristId)
        {
            if (achievementDto == null) throw new ArgumentNullException(nameof(achievementDto));
            if (touristId <= 0) throw new ArgumentException("Invalid touristId.");

            if (string.IsNullOrWhiteSpace(achievementDto.Code))
                throw new ArgumentException("Code is required.");

            if (!Enum.TryParse<AchievementCode>(achievementDto.Code, ignoreCase: true, out var code))
                throw new ArgumentException($"Invalid AchievementCode: {achievementDto.Code}");

            // Bedz se dobija jednom
            if (_achievementRepository.Has(touristId, code))
                throw new InvalidOperationException("Achievement already awarded.");

            var achievement = new Achievement(touristId, code);
            var created = _achievementRepository.Create(achievement);

            var dto = _mapper.Map<AchievementDto>(created);

            // pošto DTO ima string Code + Name/Description, popuni ručno
            dto.Code = created.Code.ToString();
            (dto.Name, dto.Description) = GetMeta(created.Code);

            return dto;
        }

        private static (string Name, string Description) GetMeta(AchievementCode code)
        {
            return code switch
            {
                AchievementCode.FirstTourCompleted =>
                    ("First Tour Completed", "You have completed your first tour."),

                AchievementCode.FiveToursCompleted =>
                    ("Five Tours Completed", "You have completed 5 tours."),

                AchievementCode.TenToursCompleted =>
                    ("Ten Tours Completed", "You have completed 10 tours."),

                AchievementCode.FirstClubJoined =>
                    ("First Club Joined", "You have joined your first club."),

                AchievementCode.FiveClubsJoined =>
                    ("Five Clubs Joined", "You have joined 5 clubs."),

                AchievementCode.TenClubsJoined =>
                    ("Ten Clubs Joined", "You have joined 10 clubs."),

                AchievementCode.FirstTourReviewWritten =>
                    ("First Tour Review Written", "You have written your first tour review."),

                AchievementCode.FiveTourReviewsWritten =>
                    ("Five Tour Reviews Written", "You have written 5 tour reviews."),

                AchievementCode.TenTourReviewsWritten =>
                    ("Ten Tour Reviews Written", "You have written 10 tour reviews."),

                AchievementCode.FirstTourBought =>
                    ("First Tour Purchased", "You have purchased your first tour."),

                AchievementCode.FiveTourBought =>
                    ("Five Tours Purchased", "You have purchased 5 tours."),

                AchievementCode.TenTourBought =>
                    ("Ten Tours Purchased", "You have purchased 10 tours."),

                _ =>
                    ("Achievement Unlocked", "You have unlocked a new achievement.")
            };
        }
    }
}
