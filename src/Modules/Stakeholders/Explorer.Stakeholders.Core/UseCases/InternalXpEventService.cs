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

        public InternalXpEventService(IXpEventService xpEventService)
        {
            _xpEventService = xpEventService;
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
            }
            catch (InvalidOperationException)
            {
                // duplikat -> ignoriši
            }
        }
    }
}
