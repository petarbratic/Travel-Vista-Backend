using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Explorer.Stakeholders.Core.Domain.RepositoryInterfaces
{
    public interface IAchievementRepository
    {
        Achievement Create(Achievement achievement);
        bool Has(long touristId, AchievementCode code);
        IReadOnlyList<Achievement> GetByTouristId(long touristId);
    }
}
