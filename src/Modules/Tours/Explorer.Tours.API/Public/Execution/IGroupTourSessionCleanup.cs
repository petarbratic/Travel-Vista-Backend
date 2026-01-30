using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Explorer.Tours.API.Public.Execution
{
    public interface IGroupTourSessionCleanup
    {
        void HandleAbandon(long tourExecutionId);
        void HandleComplete(long tourExecutionId);
    }
}
