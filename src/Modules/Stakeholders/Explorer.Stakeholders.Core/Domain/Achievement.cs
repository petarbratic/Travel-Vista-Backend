using Explorer.BuildingBlocks.Core.Domain;

namespace Explorer.Stakeholders.Core.Domain
{
    public enum AchievementCode
    {
        FirstTourCompleted,
        FiveToursCompleted,
        TenToursCompleted,
        FirstClubJoined,
        FiveClubsJoined,
        TenClubsJoined,
        FirstReviewWritten,
        FiveReviewsWritten,
        TenReviewsWritten,

        //First-time achievements
        FirstProfilePictureSet,
        FirstAppReview,
        FirstBlogCreated
    }

    public class Achievement : Entity
    {
        public long TouristId { get; private set; }
        public AchievementCode Code { get; private set; }
        public DateTime AwardedAtUtc { get; private set; }

        private Achievement() { }

        public Achievement(long touristId, AchievementCode code)
        {
            if (touristId == 0) throw new ArgumentException("Invalid TouristId.");
            TouristId = touristId;
            Code = code;
            AwardedAtUtc = DateTime.UtcNow;
        }
    }
}