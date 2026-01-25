using Explorer.Stakeholders.API.Dtos;
using Explorer.Stakeholders.API.Internal;
using Explorer.Stakeholders.API.Public;
using Explorer.Stakeholders.Core.Domain;
using Explorer.Stakeholders.Core.Domain.RepositoryInterfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Explorer.Stakeholders.Core.UseCases
{
    public class WalletService : IWalletService, IInternalWalletService
    {
        private readonly IWalletRepository _walletRepo;
        private readonly IPersonRepository _personRepo;
        private readonly IUserRepository _userRepo;
        private readonly IWalletTransactionRepository _txRepo;

        public WalletService(IWalletRepository walletRepo, IPersonRepository personRepo, IUserRepository userRepo, IWalletTransactionRepository txRepo)
        {
            _walletRepo = walletRepo;
            _personRepo = personRepo;
            _userRepo = userRepo;
            _txRepo = txRepo;
        }

        public WalletDto GetMyWallet(long personId)
        {
            EnsureRoleByPersonId(personId, UserRole.Tourist);

            var wallet = _walletRepo.GetByPersonId(personId) ?? _walletRepo.Create(new Wallet(personId));
            return new WalletDto { PersonId = personId, BalanceAc = wallet.BalanceAc };
        }

        public WalletDto TopUp(long adminPersonId, long touristUserId, int amountAc)
        {
            EnsureRoleByPersonId(adminPersonId, UserRole.Administrator);

            if (amountAc <= 0) throw new ArgumentException("Amount must be > 0.");

            var touristPerson = _personRepo.GetByUserId(touristUserId);
            if (touristPerson == null) throw new KeyNotFoundException("Tourist person not found.");

            EnsureRoleByPersonId(touristPerson.Id, UserRole.Tourist);

            var wallet = _walletRepo.GetByPersonId(touristPerson.Id) ?? _walletRepo.Create(new Wallet(touristPerson.Id));
            wallet.AddAc(amountAc);
            _walletRepo.Update(wallet);

            _txRepo.Create(new WalletTransaction(
                personId: touristPerson.Id,
                amountAc: amountAc,
                type: WalletTransactionType.AdminTopUp,
                description: $"Admin top-up: +{amountAc} AC",
                referenceType: "AdminTopUp",
                referenceId: null,
                initiatorPersonId: adminPersonId
            ));


            return new WalletDto { PersonId = touristPerson.Id, BalanceAc = wallet.BalanceAc };
        }

        // Interni servisi za Payments modul
        public WalletDto GetWallet(long personId)
        {
            var wallet = _walletRepo.GetByPersonId(personId) ?? _walletRepo.Create(new Wallet(personId));
            return new WalletDto { PersonId = personId, BalanceAc = wallet.BalanceAc };
        }

        public WalletDto DeductAc(long personId, decimal amountAc)
        {
            if (amountAc <= 0) throw new ArgumentException("Amount must be > 0.");

            var wallet = _walletRepo.GetByPersonId(personId);
            if (wallet == null) throw new KeyNotFoundException("Wallet not found.");

            if (wallet.BalanceAc < amountAc)
                throw new InvalidOperationException("Insufficient balance.");

            wallet.DeductAc((int)amountAc);
            _walletRepo.Update(wallet);

            return new WalletDto { PersonId = personId, BalanceAc = wallet.BalanceAc };
        }

        private void EnsureRoleByPersonId(long personId, UserRole role)
        {
            var person = _personRepo.Get(personId) ?? throw new KeyNotFoundException("Person not found.");
            var user = _userRepo.Get(person.UserId) ?? throw new KeyNotFoundException("User not found.");
            if (user.Role != role) throw new UnauthorizedAccessException("Not allowed.");
        }

        public PagedResultDto<WalletTransactionDto> GetMyTransactions(long personId, int page, int pageSize)
        {
            EnsureRoleByPersonId(personId, UserRole.Tourist);

            if (page <= 0) page = 1;
            if (pageSize <= 0) pageSize = 10;
            if (pageSize > 50) pageSize = 50;

            var skip = (page - 1) * pageSize;

            var total = _txRepo.CountByPersonId(personId);
            var items = _txRepo.GetByPersonId(personId, skip, pageSize)
                .Select(t => new WalletTransactionDto
                {
                    Id = t.Id,
                    CreatedAtUtc = t.CreatedAtUtc,
                    AmountAc = t.AmountAc,
                    Type = (int)t.Type,
                    Description = t.Description,
                    ReferenceType = t.ReferenceType,
                    ReferenceId = t.ReferenceId
                })
                .ToList();

            return new PagedResultDto<WalletTransactionDto>
            {
                Items = items,
                TotalCount = total,
                Page = page,
                PageSize = pageSize
            };
        }

        public WalletDto Debit(long personId, int amountAc, int type,
    string description, string? referenceType = null, long? referenceId = null, long? initiatorPersonId = null)
        {
            if (amountAc <= 0) throw new ArgumentException("Amount must be > 0.");
            if (!Enum.IsDefined(typeof(WalletTransactionType), type))
                throw new ArgumentException("Invalid transaction type.");

            var wallet = _walletRepo.GetByPersonId(personId) ?? throw new KeyNotFoundException("Wallet not found.");

            wallet.DeductAc(amountAc);
            _walletRepo.Update(wallet);

            var domainType = (WalletTransactionType)type;

            _txRepo.Create(new WalletTransaction(
                personId: personId,
                amountAc: -amountAc,
                type: domainType,
                description: description,
                referenceType: referenceType,
                referenceId: referenceId,
                initiatorPersonId: initiatorPersonId
            ));

            return new WalletDto { PersonId = personId, BalanceAc = wallet.BalanceAc };
        }

    }
}
