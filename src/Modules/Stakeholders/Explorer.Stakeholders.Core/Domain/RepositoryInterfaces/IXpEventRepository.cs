using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Explorer.Stakeholders.Core.Domain.RepositoryInterfaces
{
    public interface IXpEventRepository
    {
        XpEvent Create(XpEvent xpEvent);

        bool Exists(long touristId, XpEventType type, long sourceEntityId);

        int CountByType(long touristId, XpEventType type);

    }
}
