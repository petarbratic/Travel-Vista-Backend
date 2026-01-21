using Explorer.Stakeholders.API.Dtos;
using Explorer.Stakeholders.API.Internal;
using Explorer.Stakeholders.API.Public;
using Explorer.Stakeholders.Core.Domain;
using Explorer.Stakeholders.Core.Domain.RepositoryInterfaces;

namespace Explorer.Stakeholders.Core.UseCases;

public class WelcomeBonusService : IWelcomeBonusService, IInternalWelcomeBonusService
{
    private readonly IWelcomeBonusRepository _bonusRepository;
    private readonly IWalletRepository _walletRepository;
    private static readonly Random _random = new Random();

    public WelcomeBonusService(IWelcomeBonusRepository bonusRepository, IWalletRepository walletRepository)
    {
        _bonusRepository = bonusRepository;
        _walletRepository = walletRepository;
    }

    public WelcomeBonusDto GetWelcomeBonus(long personId)
    {
        var bonus = _bonusRepository.GetByPersonId(personId);
        if (bonus == null)
            throw new KeyNotFoundException("Welcome bonus not found.");
        
        return MapToDto(bonus);
    }

    public WelcomeBonusDto CreateWelcomeBonus(long personId)
    {
        // Proveri da li već postoji bonus za ovog korisnika
        if (_bonusRepository.ExistsForPerson(personId))
        {
            return GetWelcomeBonus(personId);
        }

        // Nasumično izvuci bonus
        var bonusType = DrawRandomBonusType();
        var bonus = new WelcomeBonus(personId, bonusType);
        
        var createdBonus = _bonusRepository.Create(bonus);

        // Ako je AC bonus, dodaj novac u wallet
        if (createdBonus.IsAcBonus())
        {
            var wallet = _walletRepository.GetByPersonId(personId);
            if (wallet != null)
            {
                wallet.AddAc(createdBonus.Value);
                _walletRepository.Update(wallet);
            }
        }

        return MapToDto(createdBonus);
    }

    public WelcomeBonusDto? GetActiveDiscountBonus(long personId)
    {
        var bonus = _bonusRepository.GetByPersonId(personId);
        
        if (bonus == null || bonus.IsUsed || bonus.IsExpired() || !bonus.IsDiscountBonus())
            return null;
        
        return MapToDto(bonus);
    }

    public void MarkBonusAsUsed(long personId)
    {
        var bonus = _bonusRepository.GetByPersonId(personId);
        
        if (bonus == null)
            throw new KeyNotFoundException("Welcome bonus not found.");
        
        if (bonus.IsUsed)
            return; // Already used, no need to throw
        
        if (bonus.IsExpired())
            throw new InvalidOperationException("Bonus has expired.");
        
        bonus.MarkAsUsed();
        _bonusRepository.Update(bonus);
    }

    private BonusType DrawRandomBonusType()
    {
        // Verovatnoće:
        // AC100 = 30%, AC250 = 20%, AC500 = 10%
        // Discount10 = 20%, Discount20 = 15%, Discount30 = 5%
        var roll = _random.Next(1, 101); // 1-100

        if (roll <= 30) return BonusType.AC100;           // 1-30 (30%)
        if (roll <= 50) return BonusType.AC250;           // 31-50 (20%)
        if (roll <= 60) return BonusType.AC500;           // 51-60 (10%)
        if (roll <= 80) return BonusType.Discount10;      // 61-80 (20%)
        if (roll <= 95) return BonusType.Discount20;      // 81-95 (15%)
        return BonusType.Discount30;                       // 96-100 (5%)
    }

    private WelcomeBonusDto MapToDto(WelcomeBonus bonus)
    {
        return new WelcomeBonusDto
        {
            Id = bonus.Id,
            PersonId = bonus.PersonId,
            BonusType = (int)bonus.BonusType,
            Value = bonus.Value,
            IsUsed = bonus.IsUsed,
            CreatedAt = bonus.CreatedAt,
            ExpiresAt = bonus.ExpiresAt,
            UsedAt = bonus.UsedAt
        };
    }
}
