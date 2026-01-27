using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Explorer.Tours.API.Dtos;

namespace Explorer.Tours.API.Public.Execution;

public interface ITourHistoryService
{
    TourHistoryOverviewDto GetTourHistory(long touristId);
}
