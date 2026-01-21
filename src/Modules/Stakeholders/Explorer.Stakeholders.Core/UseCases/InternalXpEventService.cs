using Explorer.Stakeholders.API.Dtos;
using Explorer.Stakeholders.API.Internal;
using Explorer.Stakeholders.API.Public;
using Explorer.Stakeholders.Core.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Explorer.Stakeholders.Core.UseCases
{
    public class InternalXpEventService : IInternalXpEventService
    {
        private readonly IXpEventService _xpEventService;
        private readonly IInternalTouristXPAndLevelSerive _internalTouristXPAndLevelSerive;

        public InternalXpEventService(IXpEventService xpEventService, IInternalTouristXPAndLevelSerive internalTouristXPAndLevelSerive)
        {
            _xpEventService = xpEventService;
            _internalTouristXPAndLevelSerive = internalTouristXPAndLevelSerive;
        }

        public void CreateTourReviewXp(long touristId, long tourId, int amount)
        {
            // best effort + idempotentnost je u XpEventService (Exists check)
            try
            {
                var dto = new XpEventDto
                {
                    SourceEntityId = tourId,
                    Amount = amount,
                    Type = XpEventType.TourReviewWritten.ToString()
                };

                _xpEventService.Create(dto, touristId);
                _internalTouristXPAndLevelSerive.AddExperience(touristId, dto.Amount);
            }
            catch (InvalidOperationException)
            {
                // duplikat -> ignoriši
            }

        }
        public void BuyTourXp(long touristId, long tourId, int amount)
        {
            // best effort + idempotentnost je u XpEventService (Exists check)
            try
            {
                var dto = new XpEventDto
                {
                    SourceEntityId = tourId,
                    Amount = amount,
                    Type = XpEventType.TourBought.ToString()
                };

                _xpEventService.Create(dto, touristId);
                _internalTouristXPAndLevelSerive.AddExperience(touristId, dto.Amount);
            }
            catch (InvalidOperationException)
            {
                // duplikat -> ignoriši
            }

        }
        public void CreateTourCompletedXp(long touristId, long tourId, int amount)
        {
            // best effort + idempotentnost je u XpEventService (Exists check)
            try
            {
                var dto = new XpEventDto
                {
                    SourceEntityId = tourId,
                    Amount = amount,
                    Type = XpEventType.TourCompleted.ToString()
                };

                _xpEventService.Create(dto, touristId);
                _internalTouristXPAndLevelSerive.AddExperience(touristId, dto.Amount);
            }
            catch (InvalidOperationException)
            {
                // duplikat -> ignoriši
            }

        }
    }
}
