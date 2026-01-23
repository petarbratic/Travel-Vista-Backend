using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Explorer.Stakeholders.Core.Domain.RepositoryInterfaces;
using Explorer.Stakeholders.Core.Domain;

namespace Explorer.Stakeholders.Infrastructure.Database.Repositories
{
    public class XpEventDbRepository : IXpEventRepository
    {
        private readonly StakeholdersContext _context;

        public XpEventDbRepository(StakeholdersContext context)
        {
            _context = context;
        }

        public XpEvent Create(XpEvent xpEvent)
        {
            _context.XpEvents.Add(xpEvent);
            _context.SaveChanges();
            return xpEvent;
        }
        public bool Exists(long touristId, XpEventType type, long sourceEntityId)
        {
            return _context.XpEvents.Any(e =>
                e.TouristId == touristId &&
                e.Type == type &&
                e.SourceEntityId == sourceEntityId);
        }

        public int CountByType(long touristId, XpEventType type)
        {
            return _context.XpEvents.Count(e =>
                e.TouristId == touristId &&
                e.Type == type);
        }


    }
}
