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
    public class XpEventService : IXpEventService
    {
        private readonly IXpEventRepository _xpEventRepository;
        private IMapper _mapper;


        public XpEventService(IXpEventRepository xpEventrepository, IMapper mapper)
        {
            _xpEventRepository = xpEventrepository;
            _mapper = mapper;
        }

        public XpEventDto Create(XpEventDto eventDto, long touristId)
        {
            if (eventDto == null) throw new ArgumentNullException(nameof(eventDto));
            if (touristId == 0) throw new ArgumentException("Invalid touristId.");

            if (eventDto.Amount <= 0)
                throw new ArgumentException("Amount must be > 0.");

            if (eventDto.SourceEntityId == 0)
                throw new ArgumentException("SourceEntityId must be > 0.");

            if (string.IsNullOrWhiteSpace(eventDto.Type))
                throw new ArgumentException("Type is required.");

            // Parsiranje stringa u enum
            if (!Enum.TryParse<XpEventType>(eventDto.Type, ignoreCase: true, out var type))
                throw new ArgumentException($"Invalid XpEventType: {eventDto.Type}");

            // Idempotentnost: ne sme duplo za isti source
            if (_xpEventRepository.Exists(touristId, type, eventDto.SourceEntityId))
                throw new InvalidOperationException("XP event already exists for this action.");

            var xpEvent = new XpEvent(
                touristId,
                type,
                eventDto.Amount,
                eventDto.SourceEntityId
            );

            var created = _xpEventRepository.Create(xpEvent);

            // Map domen -> dto (ručno dopuni polja koja se ne mapiraju automatski)
            var result = _mapper.Map<XpEventDto>(created);
            result.Type = created.Type.ToString();
            result.Description = GetDescription(created.Type, created.Amount);

            return result;
        }

        private static string GetDescription(XpEventType type, int amount)
        {
            return type switch
            {
                XpEventType.TourCompleted => $"+{amount} XP - Tour completed",
                XpEventType.ClubJoined => $"+{amount} XP - Club joined",
                XpEventType.ReviewWritten => $"+{amount} XP - Review written",
                _ => $"+{amount} XP"
            };
        }
    }
}
