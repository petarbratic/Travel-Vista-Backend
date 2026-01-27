using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Explorer.Tours.API.Dtos
{
    public class TourHistoryDto
    {
        public long ExecutionId { get; set; }
        public long TourId { get; set; }
        public string TourName { get; set; }
        public DateTime CompletedAt { get; set; }
        public string Location { get; set; } // Prva key point lokacija
        public double DistanceInKm { get; set; }
        public double DurationInMinutes { get; set; }

    }
}
