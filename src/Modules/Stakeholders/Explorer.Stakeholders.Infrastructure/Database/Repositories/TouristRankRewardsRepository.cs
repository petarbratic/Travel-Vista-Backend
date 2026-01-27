using Explorer.Stakeholders.Core.Domain;
using Explorer.Stakeholders.Core.Domain.RepositoryInterfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Explorer.Stakeholders.Infrastructure.Database.Repositories;

public class TouristRankRewardsRepository : ITouristRankRewardsRepository
{
    private readonly StakeholdersContext _context;

    public TouristRankRewardsRepository(StakeholdersContext context)
    {
        _context = context;
    }

    public TouristRankRewards? GetByTouristId(long touristId)
    {
        return _context.TouristRankRewards
            .FirstOrDefault(r => r.TouristId == touristId);
    }

    public TouristRankRewards Create(TouristRankRewards rewards)
    {
        _context.TouristRankRewards.Add(rewards);
        _context.SaveChanges();
        return rewards;
    }

    public TouristRankRewards Update(TouristRankRewards rewards)
    {
        _context.TouristRankRewards.Update(rewards);
        _context.SaveChanges();
        return rewards;
    }
}