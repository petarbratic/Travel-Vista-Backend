using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Explorer.Stakeholders.Core.Domain.RepositoryInterfaces
{
    public interface IWalletTransactionRepository
    {
        WalletTransaction Create(WalletTransaction tx);
        IEnumerable<WalletTransaction> GetByPersonId(long personId, int skip, int take);
        int CountByPersonId(long personId);
    }
}
