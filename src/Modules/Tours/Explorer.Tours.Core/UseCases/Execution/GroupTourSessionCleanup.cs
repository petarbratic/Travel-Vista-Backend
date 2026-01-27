using Explorer.Tours.API.Public.Execution;
using Explorer.Tours.Core.Domain.RepositoryInterfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Explorer.Tours.Core.UseCases.Execution
{
    public class GroupTourSessionCleanup : IGroupTourSessionCleanup
    {
        private readonly IGroupTourSessionRepository _groupRepo;

        public GroupTourSessionCleanup(IGroupTourSessionRepository groupSessionRepository)
        {
            _groupRepo = groupSessionRepository;
        }

        public void HandleAbandon(long tourExecutionId)
        {
            var participant = _groupRepo.FindParticipantByTourExecutionId(tourExecutionId);
            if (participant == null) return;

            var session = _groupRepo.FindById(participant.SessionId);

            if (session == null)
                throw new KeyNotFoundException("Group tour session not found");

            if (!participant.HasLeft) {
                session.LeaveParticipant(participant.TouristId);
                _groupRepo.Update(session);
                _groupRepo.UpdateParticipant(participant);
            }
        }

        public void HandleComplete(long tourExecutionId)
        {
            HandleAbandon(tourExecutionId);
        }
    }
}
