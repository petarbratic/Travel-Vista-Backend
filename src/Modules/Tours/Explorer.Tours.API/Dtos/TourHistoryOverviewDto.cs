using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Explorer.Tours.API.Dtos
{
    public class TourHistoryOverviewDto
    {
        public List<TourHistoryDto> CompletedTours { get; set; }
        public TourStatisticsDto Statistics { get; set; }
        public TourComparisonDto Comparison { get; set; }

    }
}
