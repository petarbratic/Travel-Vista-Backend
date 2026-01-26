using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Explorer.Tours.API.Dtos
{
    public class TourStatisticsDto
    {
        public int TotalCompletedTours { get; set; }
        public double TotalDistanceTraveled { get; set; } // km
        public double TotalTimeSpent { get; set; } // minutes
        public double CompletionRate { get; set; } // % (completed / purchased)

    }
}
