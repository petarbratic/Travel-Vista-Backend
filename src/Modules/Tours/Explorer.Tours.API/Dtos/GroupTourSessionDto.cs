using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Explorer.Tours.API.Dtos
{
    public class GroupTourSessionDto
    {
        public long Id { get; set; }
        public long TourId { get; set; }
        public long ClubId { get; set; }
        public int Status { get; set; }
        public DateTime StartTime { get; set; }
        public long StarterId { get; set; }

        public List<GroupTourSessionParticipantDto> Participants { get; set; } = new List<GroupTourSessionParticipantDto>();
    }
}
