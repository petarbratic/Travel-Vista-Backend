using System.Collections.Generic;

namespace Explorer.Stakeholders.API.Internal;

public interface IInternalTouristService
{
    List<long> GetAllTouristPersonIds();
}
