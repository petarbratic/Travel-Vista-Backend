using Explorer.Stakeholders.API.Internal;
using Explorer.Stakeholders.Core.Domain.RepositoryInterfaces;
using System.Collections.Generic;
using System.Linq;

namespace Explorer.Stakeholders.Core.UseCases;

public class InternalTouristService : IInternalTouristService
{
    private readonly ITouristRepository _touristRepository;

    public InternalTouristService(ITouristRepository touristRepository)
    {
        _touristRepository = touristRepository;
    }

    public List<long> GetAllTouristPersonIds()
    {
        var tourists = _touristRepository.GetAll();
        return tourists.Select(t => t.PersonId).ToList();
    }
}
