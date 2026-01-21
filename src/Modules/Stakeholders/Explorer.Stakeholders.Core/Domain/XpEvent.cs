using Explorer.BuildingBlocks.Core.Domain;

namespace Explorer.Stakeholders.Core.Domain
{
    public enum XpEventType
    {
        TourCompleted,
        ClubJoined,
        ReviewWritten,
        //First-time akcije
        FirstProfilePictureSet,
        FirstAppReview,
        FirstClubJoined,
        FirstBlogCreated
    }

    public class XpEvent : Entity
    {
        public long TouristId { get; private set; }
        public XpEventType Type { get; private set; }
        public int Amount { get; private set; }
        public DateTime CreatedAtUtc { get; private set; }
        public long SourceEntityId { get; private set; }

        private XpEvent() { }

        public XpEvent(long touristId, XpEventType type, int amount, long sourceEntityId)
        {
            if (touristId == 0) throw new ArgumentException("Invalid TouristId.");
            if (amount <= 0) throw new ArgumentException("Amount must be > 0.");
            if (sourceEntityId == 0) throw new ArgumentException("Invalid SourceEntityId.");

            TouristId = touristId;
            Type = type;
            Amount = amount;
            CreatedAtUtc = DateTime.UtcNow;
            SourceEntityId = sourceEntityId;
        }
    }
}