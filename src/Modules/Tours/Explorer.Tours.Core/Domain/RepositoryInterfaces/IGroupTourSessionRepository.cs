using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Explorer.Tours.Core.Domain.RepositoryInterfaces
{
    public interface IGroupTourSessionRepository
    {
        GroupTourSession Update(GroupTourSession session);
        GroupTourSession? FindById(long id);
        List<GroupTourSession> FindActiveByClubId(long clubId);
        List<GroupTourSession> FindHighlightedByClubId(long clubId);
        List<GroupTourSession> FindSessionsForHighlightMarking(long clubId);
        List<GroupTourSession> FindByClubId(long clubId);
        List<GroupTourSessionParticipant>? FindOtherGroupParticipantsByTouristId(long touristId);

        GroupTourSession CreateGroupTourSession(GroupTourSession session);
        GroupTourSessionParticipant AddParticipant(GroupTourSessionParticipant participant);
        GroupTourSessionParticipant UpdateParticipant(GroupTourSessionParticipant participant);
        GroupTourSessionParticipant FindParticipantByTourExecutionId(long tourExecutionId);
    }
}
