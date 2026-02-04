using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Explorer.Stakeholders.API.Internal
{
    public interface IInternalXpEventService
    {
        void CreateTourReviewXp(long touristId, long tourId, int amount);
        void BuyTourXp(long touristId, long tourId, int amount);
        void CreateTourCompletedXp(long touristId, long tourId, int amount);
    }
}
