using Explorer.Stakeholders.API.Dtos;
using Explorer.Stakeholders.API.Internal;
using Explorer.Stakeholders.Core.Domain.RepositoryInterfaces;

namespace Explorer.Stakeholders.Core.UseCases;

public class InternalTouristXPAndLevelService : IInternalTouristXPAndLevelSerive
{
    private readonly ITouristRepository _touristRepository;

    public InternalTouristXPAndLevelService(ITouristRepository touristRepository)
    {
        _touristRepository = touristRepository;
    }

    public void AddExperience(long touristId, int xp)
    {
        var tourist = _touristRepository.Get(touristId);
        if (tourist == null)
            throw new KeyNotFoundException($"Tourist with id {touristId} not found.");

        tourist.IncreaseXP(xp);
        _touristRepository.Update(tourist);
    }
    public int GetLevel(long touristId)
    {
        var tourist = _touristRepository.Get(touristId);
        if (tourist == null)
            throw new KeyNotFoundException($"Tourist with id {touristId} not found.");
        return tourist.Level;
    }

    public TouristStatsDto GetTouristStats(long touristId)
    {
        var tourist = _touristRepository.Get(touristId);
        if (tourist == null)
            throw new KeyNotFoundException($"Tourist with id {touristId} not found.");

        return new TouristStatsDto
        {
            TouristId = tourist.Id,
            XP = tourist.XP,
            Level = tourist.Level,
            Rank = tourist.Rank.ToString()
        };
    }
    public TouristStatsDto GetStatsByPersonId(long personId)
    {
        var tourist = _touristRepository.Get(personId);
        if (tourist == null)
            throw new KeyNotFoundException($"Tourist with PersonId {personId} not found.");

        return new TouristStatsDto
        {
            TouristId = tourist.Id,
            XP = tourist.XP,
            Level = tourist.Level,
            Rank = tourist.Rank.ToString()
        };
    }
}
