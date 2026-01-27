using Explorer.Stakeholders.Core.Domain;
using Explorer.Stakeholders.Core.Domain.RepositoryInterfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Explorer.Stakeholders.Infrastructure.Database.Repositories
{
    public class WalletTransactionDbRepository : IWalletTransactionRepository
    {
        private readonly StakeholdersContext _context;
        public WalletTransactionDbRepository(StakeholdersContext context) => _context = context;

        public WalletTransaction Create(WalletTransaction tx)
        {
            _context.WalletTransactions.Add(tx);
            _context.SaveChanges();
            return tx;
        }

        public IEnumerable<WalletTransaction> GetByPersonId(long personId, int skip, int take)
        {
            return _context.WalletTransactions
                .Where(t => t.PersonId == personId)
                .OrderByDescending(t => t.CreatedAtUtc)
                .Skip(skip)
                .Take(take)
                .ToList();
        }

        public int CountByPersonId(long personId)
            => _context.WalletTransactions.Count(t => t.PersonId == personId);
    }
}
