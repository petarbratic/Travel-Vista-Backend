using Explorer.BuildingBlocks.Core.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        TenReviewsWritten
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

