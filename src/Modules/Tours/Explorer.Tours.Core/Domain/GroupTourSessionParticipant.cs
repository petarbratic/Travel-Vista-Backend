using Explorer.BuildingBlocks.Core.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Explorer.Tours.Core.Domain
{
    public class GroupTourSessionParticipant : Entity
    {
        public long SessionId { get; private set; }
        public long TouristId { get; private set; }
        public DateTime JoinedAt { get; private set; }
        public DateTime? LeftAt { get; private set; }
        public long TourExecutionId { get; private set; }

        private GroupTourSessionParticipant() { }

        public GroupTourSessionParticipant(long sessionId, long touristId, long tourExecutionId)
        {
            if (sessionId == 0)
                throw new ArgumentException("Session ID must be valid.", nameof(sessionId));
            if (touristId == 0)
                throw new ArgumentException("Tourist ID must be valid.", nameof(touristId));
            if (tourExecutionId == 0)
                throw new ArgumentException("Tour Execution ID must be valid.", nameof(tourExecutionId));
            SessionId = sessionId;
            TouristId = touristId;
            TourExecutionId = tourExecutionId;
            JoinedAt = DateTime.UtcNow;
        }

        public void LeaveSession()
        {
            if (LeftAt != null)
                throw new InvalidOperationException("Participant has already left the session.");
            LeftAt = DateTime.UtcNow;
        }

        public bool HasLeft
        {
            get { return LeftAt != null; }
        }
    }
}
