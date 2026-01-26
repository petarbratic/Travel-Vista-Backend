using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Explorer.Tours.API.Dtos
{
    public class TourComparisonDto
    {
        public int YourCompletedTours { get; set; }
        public int AverageCompletedTours { get; set; }
        public double ToursPercentageDifference { get; set; } // +15% or -10%

        public double YourCompletionRate { get; set; }
        public double AverageCompletionRate { get; set; }
        public double CompletionRateDifference { get; set; }

        public double YourTotalDistance { get; set; }
        public double AverageTotalDistance { get; set; }
        public double DistancePercentageDifference { get; set; }

    }
}
