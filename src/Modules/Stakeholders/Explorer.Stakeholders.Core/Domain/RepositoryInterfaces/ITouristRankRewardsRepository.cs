using Explorer.Stakeholders.Core.Domain;

namespace Explorer.Stakeholders.Core.Domain.RepositoryInterfaces;

public interface ITouristRankRewardsRepository
{
    TouristRankRewards? GetByTouristId(long touristId);
    TouristRankRewards Create(TouristRankRewards rewards);
    TouristRankRewards Update(TouristRankRewards rewards);
}