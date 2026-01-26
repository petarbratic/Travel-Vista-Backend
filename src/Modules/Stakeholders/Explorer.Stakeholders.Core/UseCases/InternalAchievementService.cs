using Explorer.Stakeholders.API.Internal;
using Explorer.Stakeholders.Core.Domain;
using Explorer.Stakeholders.Core.Domain.RepositoryInterfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Explorer.Stakeholders.Core.UseCases
{
    public class InternalAchievementService : IInternalAchievementService
    {
        private readonly IAchievementRepository _achievementRepository;
        private readonly IXpEventRepository _xpEventRepository;

        public InternalAchievementService(IAchievementRepository achievementRepository, IXpEventRepository xpEventRepository)
        {
            _achievementRepository = achievementRepository;
            _xpEventRepository = xpEventRepository;
        }

        public string BoughtTours(long touristId)
        {
            var boughtCount = _xpEventRepository.CountByType(touristId, Domain.XpEventType.TourBought);

            var newlyUnlocked = new List<AchievementCode>();

            if (boughtCount >= 1 && !_achievementRepository.Has(touristId, AchievementCode.FirstTourBought))
                newlyUnlocked.Add(AchievementCode.FirstTourBought);

            if (boughtCount >= 5 && !_achievementRepository.Has(touristId, AchievementCode.FiveTourBought))
                newlyUnlocked.Add(AchievementCode.FiveTourBought);

            if (boughtCount >= 10 && !_achievementRepository.Has(touristId, AchievementCode.TenTourBought))
                newlyUnlocked.Add(AchievementCode.TenTourBought);

            
            foreach (var code in newlyUnlocked)
                _achievementRepository.Create(new Achievement(touristId, code));

            if (!newlyUnlocked.Any()) return "";

            return string.Join(", ", newlyUnlocked.Select(c => c.ToString()));
        }

        public string CompletedTours(long touristId)
        {
            var completedCount = _xpEventRepository.CountByType(touristId, Domain.XpEventType.TourCompleted);

            var newlyUnlocked = new List<AchievementCode>();

            if (completedCount >= 1 && !_achievementRepository.Has(touristId, AchievementCode.FirstTourCompleted))
                newlyUnlocked.Add(AchievementCode.FirstTourCompleted);

            if (completedCount >= 5 && !_achievementRepository.Has(touristId, AchievementCode.FiveToursCompleted))
                newlyUnlocked.Add(AchievementCode.FiveToursCompleted);

            if (completedCount >= 10 && !_achievementRepository.Has(touristId, AchievementCode.TenToursCompleted))
                newlyUnlocked.Add(AchievementCode.TenToursCompleted);

            foreach (var code in newlyUnlocked)
                _achievementRepository.Create(new Achievement(touristId, code));

            if (!newlyUnlocked.Any()) return "";

            return string.Join(", ", newlyUnlocked.Select(c => c.ToString()));
        }
        public string ClubsJoined(long touristId)
        {
            // Event type: jedan dogadjaj kad se uclani u klub
            var joinedCount = _xpEventRepository.CountByType(touristId, Domain.XpEventType.FirstClubJoined);

            // Ako se nije uclanio nijednom, nista
            if (joinedCount < 1)
                return "";

            // Otkljucaj jednom
            //_achievementRepository.Create(new Achievement(touristId, AchievementCode.FirstClubJoined));
            return AchievementCode.FirstClubJoined.ToString();
        }
        public string TourReviewsWritten(long touristId)
        {
            var reviewCount = _xpEventRepository.CountByType(touristId, Domain.XpEventType.TourReviewWritten);

            var newlyUnlocked = new List<AchievementCode>();

            if (reviewCount >= 1 && !_achievementRepository.Has(touristId, AchievementCode.FirstReviewWritten))
                newlyUnlocked.Add(AchievementCode.FirstReviewWritten);

            if (reviewCount >= 5 && !_achievementRepository.Has(touristId, AchievementCode.FiveReviewsWritten))
                newlyUnlocked.Add(AchievementCode.FiveReviewsWritten);

            if (reviewCount >= 10 && !_achievementRepository.Has(touristId, AchievementCode.TenReviewsWritten))
                newlyUnlocked.Add(AchievementCode.TenReviewsWritten);

            foreach (var code in newlyUnlocked)
                _achievementRepository.Create(new Achievement(touristId, code));

            if (!newlyUnlocked.Any()) return "";

            return string.Join(", ", newlyUnlocked.Select(c => c.ToString()));
        }

        public string ProfilePictureChanged(long touristId)
        {
            // Event type: jedan dogadjaj kad se uclani u klub
            var pictureSetCount = _xpEventRepository.CountByType(touristId, Domain.XpEventType.FirstProfilePictureSet);

            // Ako se nije uclanio nijednom, nista
            if (pictureSetCount < 1)
                return "";

            // Otkljucaj jednom
            //_achievementRepository.Create(new Achievement(touristId, AchievementCode.FirstClubJoined));
            return AchievementCode.FirstClubJoined.ToString();
        }

    }
}
