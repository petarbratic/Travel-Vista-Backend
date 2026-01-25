using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Explorer.Stakeholders.API.Internal
{
    public interface IInternalAchievementService
    {
        string BoughtTours(long touristId);
        string CompletedTours(long touristId);
        string ClubsJoined(long touristId);
        string TourReviewsWritten(long touristId);
    }
}
