using Explorer.BuildingBlocks.Core.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Explorer.Tours.Core.Domain
{
    public class GroupTourSession : AggregateRoot
    {
        public long TourId { get; private set; }
        public string TourName { get; private set; }
        public long ClubId { get; private set; }
        public GroupTourSessionStatus Status { get; private set; }
        public DateTime StartTime { get; private set; }
        public long StarterId { get; private set; }

        public bool? IsHighlighted { get; private set; } = null;

        public List<GroupTourSessionParticipant> Participants { get; private set; } = new List<GroupTourSessionParticipant>();

        private GroupTourSession() { }

        public GroupTourSession(long tourId, string tourName, long clubId, long starterId)
        {
            if (tourId == 0)
                throw new ArgumentException("Tour ID must be valid.", nameof(tourId));
            if (clubId == 0)
                throw new ArgumentException("Club ID must be valid.", nameof(clubId));
            if (starterId == 0)
                throw new ArgumentException("Starter ID must be valid.", nameof(starterId));
            TourId = tourId;
            TourName = tourName;
            ClubId = clubId;
            Status = GroupTourSessionStatus.Active;
            StartTime = DateTime.UtcNow;
            StarterId = starterId;
        }

        public void Highlight()
        {
            if (!IsEnded)
                throw new InvalidOperationException("Cannot highlight an active session.");
            IsHighlighted = true;
        }

        public void RefuseHighlight()
        {
            if (!IsEnded)
                throw new InvalidOperationException("Cannot refuse highlight for an active session.");
            IsHighlighted = false;
        }

        public GroupTourSessionParticipant JoinParticipant(long touristId, long tourExecutionId)
        {
            if (!IsEnded)
            {
                if (Participants.Any(p => p.TouristId == touristId && !p.HasLeft))
                    throw new InvalidOperationException("Participant is already in the session.");
                var participant = new GroupTourSessionParticipant(Id, touristId, tourExecutionId);
                Participants.Add(participant);

                return participant;
            }
            else
            {
                throw new InvalidOperationException("Cannot join a completed session.");
            }
        }

        public GroupTourSessionParticipant LeaveParticipant(long touristId)
        {
            var participant = Participants.FirstOrDefault(p => p.TouristId == touristId && !p.HasLeft);
            if (participant == null)
                throw new InvalidOperationException("Participant is not in the session.");
            participant.LeaveSession();
            
            // Participants.Remove(participant);

            if (Participants.Where(p => p.LeftAt == null).ToList().Count == 0)
                EndSession();

            return participant;
        }

        public void EndSession()
        {
            if (Status == GroupTourSessionStatus.Ended)
                throw new InvalidOperationException("Session is already completed.");
            Status = GroupTourSessionStatus.Ended;
        }

        public bool IsEnded
        {
            get { return Status == GroupTourSessionStatus.Ended; }
        }
    }
}
