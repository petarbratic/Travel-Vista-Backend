using Explorer.BuildingBlocks.Core.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Explorer.Stakeholders.Core.Domain
{
    public enum XpEventType
    {
        TourCompleted,
        ClubJoined,
        ReviewWritten
    }

    public class XpEvent : Entity
    {
        public long TouristId { get; private set; }
        public XpEventType Type { get; private set; }
        public int Amount { get; private set; }
        public DateTime CreatedAtUtc { get; private set; }
        public long? SourcEntityId { get; private set; } // da se ne bi desilo dupliranje xp-a

        public XpEvent() { }

        public XpEvent(long touristId, XpEventType type, int amount, long? sourceEntityId = null)
        {
            if (touristId == 0) throw new ArgumentException("Invalid TouristId.");
            if (amount <= 0) throw new ArgumentException("Amount must be > 0.");
            TouristId = touristId;
            Type = type;
            Amount = amount;
            CreatedAtUtc = DateTime.UtcNow;
            SourcEntityId = sourceEntityId;
        }
        public void Update(long touristId, XpEventType type, int amount, long? sourceEntityId = null)
        {
            if (touristId == 0) throw new ArgumentException("Invalid TouristId.");
            if (amount <= 0) throw new ArgumentException("Amount must be > 0.");
            TouristId = touristId;
            Type = type;
            Amount = amount;
            SourcEntityId = sourceEntityId;
        }
    }
}


