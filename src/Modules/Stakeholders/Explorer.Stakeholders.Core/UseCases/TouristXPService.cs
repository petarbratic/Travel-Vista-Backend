using Explorer.Stakeholders.API.Dtos;
using Explorer.Stakeholders.API.Internal;
using Explorer.Stakeholders.API.Public;
using Explorer.Stakeholders.Core.Domain;
using Explorer.Stakeholders.Core.Domain.RepositoryInterfaces;

namespace Explorer.Stakeholders.Core.UseCases;

public class TouristXPService : ITouristXPService, IInternalTouristRankService 
{
    private readonly ITouristRepository _touristRepository;

    public TouristXPService(ITouristRepository touristRepository)
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

    // Metoda iz Internal interfejsa
    public decimal GetRankDiscountPercentage(long touristId)
    {
        var tourist = _touristRepository.Get(touristId);
        if (tourist == null)
            throw new KeyNotFoundException($"Tourist with id {touristId} not found.");

        return tourist.Rank switch
        {
            TouristRank.Silver => 5m,      // 5%
            TouristRank.Gold => 5m,        // 5%
            TouristRank.Platinum => 10m,   // 10%
            TouristRank.Diamond => 20m,    // 20%
            TouristRank.Vista => 40m,      // 40%
            _ => 0m                        // Rookie i Bronze = 0%
        };
    }
}