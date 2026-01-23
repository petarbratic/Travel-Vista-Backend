using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Explorer.Tours.API.Public.Execution
{
    public interface IGroupTourExecutionService
    {
        long StartGroupExecution(long tourId, long touristId, long sessionId);
        long LeaveGroupExecution(long touristId);
        bool HasActiveExecution(long touristId);
    }
}
