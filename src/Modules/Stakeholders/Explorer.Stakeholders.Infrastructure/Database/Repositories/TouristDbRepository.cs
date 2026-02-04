using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Explorer.Stakeholders.Core.Domain;
using Explorer.Stakeholders.Core.Domain.RepositoryInterfaces;
using Microsoft.EntityFrameworkCore;

namespace Explorer.Stakeholders.Infrastructure.Database.Repositories;

public class TouristDbRepository : ITouristRepository
{
    private readonly StakeholdersContext _dbContext;
    private readonly DbSet<Tourist> _dbSet;

    public TouristDbRepository(StakeholdersContext dbContext)
    {
        _dbContext = dbContext;
        _dbSet = _dbContext.Set<Tourist>();
    }

    public Tourist Get(long touristId)
    {
        // Traži turista po Tourist.Id (ne PersonId!)
        return _dbSet.FirstOrDefault(t => t.Id == touristId);
    }

    public Tourist GetByPersonId(long personId)
    {
        // Traži turista po PersonId
        return _dbSet.FirstOrDefault(t => t.PersonId == personId);
    }

    public Tourist Create(Tourist tourist)
    {
        _dbSet.Add(tourist);
        _dbContext.SaveChanges();
        return tourist;
    }

    public Tourist Update(Tourist tourist)
    {
        var entry = _dbContext.Entry(tourist);

        // Ako je detached, attach-uj ga
        if (entry.State == EntityState.Detached)
        {
            _dbSet.Attach(tourist);
        }

        // Eksplicitno označi XP i Level kao promenjene
        entry.Property(t => t.XP).IsModified = true;
        entry.Property(t => t.Level).IsModified = true;

        _dbContext.SaveChanges();
        return tourist;
    }

    public List<Tourist> GetAll()
    {
        return _dbSet.ToList();
    }
}