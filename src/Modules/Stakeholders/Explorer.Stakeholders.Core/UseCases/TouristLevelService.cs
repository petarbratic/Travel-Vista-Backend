using Explorer.Stakeholders.API.Public;
using Explorer.Stakeholders.Core.Domain.RepositoryInterfaces;

namespace Explorer.Stakeholders.Core.UseCases
{
    public class TouristLevelService : ITouristLevelService
    {
        private readonly ITouristRepository _touristRepository; // ili kako se već zove kod vas

        public TouristLevelService(ITouristRepository touristRepository)
        {
            _touristRepository = touristRepository;
        }

        public int GetLevel(long touristId)
        {
            var tourist = _touristRepository.Get(touristId);
            if (tourist == null) throw new KeyNotFoundException($"Tourist {touristId} not found.");
            return tourist.Level;
        }
    }
}