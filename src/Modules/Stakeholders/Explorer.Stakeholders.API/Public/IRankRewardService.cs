using Explorer.Stakeholders.API.Dtos;

namespace Explorer.Stakeholders.API.Public;

public interface IRankRewardService
{
    RankRewardClaimResultDto ClaimRankRewards(long touristId);
}