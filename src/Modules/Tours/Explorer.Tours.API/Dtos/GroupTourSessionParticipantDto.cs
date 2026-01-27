using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Explorer.Tours.API.Dtos
{
    public class GroupTourSessionParticipantDto
    {
        public long SessionId { get; set; }
        public long TouristId { get; set; }
        public DateTime JoinedAt { get; set; }
        public DateTime? LeftAt { get; set; }
        public long TourExecutionId { get; set; }
        public PositionDto? Position { get; set; }
    }
}
