using Explorer.Stakeholders.API.Dtos;

namespace Explorer.Stakeholders.API.Public;

public interface ITouristXPService
{
    void AddExperience(long touristId, int xp);
    int GetLevel(long touristId);
    TouristStatsDto GetTouristStats(long touristId);
    TouristStatsDto GetStatsByPersonId(long personId);
}