using Explorer.Stakeholders.Core.Domain.RepositoryInterfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Explorer.Stakeholders.Core.Domain;

namespace Explorer.Stakeholders.Infrastructure.Database.Repositories
{
    public class AchievementDbRepository : IAchievementRepository
    {
        private readonly StakeholdersContext _context;

        public AchievementDbRepository(StakeholdersContext context)
        {
            _context = context;
        }

        public Achievement Create(Achievement achievement)
        {
            _context.Achievements.Add(achievement);
            _context.SaveChanges();
            return achievement;
        }
        public bool Has(long touristId, AchievementCode code)
        {
            return _context.Achievements.Any(a =>
                a.TouristId == touristId &&
                a.Code == code);
        }

        public IReadOnlyList<Achievement> GetByTouristId(long touristId)
        {
            return _context.Achievements
                .Where(a => a.TouristId == touristId)
                .OrderByDescending(a => a.AwardedAtUtc)
                .ToList();
        }
    }
}
