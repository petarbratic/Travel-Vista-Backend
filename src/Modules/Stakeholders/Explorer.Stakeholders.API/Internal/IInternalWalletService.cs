using Explorer.Stakeholders.API.Dtos;

namespace Explorer.Stakeholders.API.Internal
{
    public interface IInternalWalletService
    {
        WalletDto GetWallet(long personId);
        WalletDto DeductAc(long personId, decimal amountAc);

        WalletDto Debit(long personId, int amountAc, int type,
            string description, string? referenceType = null, long? referenceId = null, long? initiatorPersonId = null);
    }
}
