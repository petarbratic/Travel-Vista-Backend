using Explorer.Stakeholders.API.Dtos;
using Explorer.Stakeholders.API.Internal;
using Explorer.Stakeholders.API.Public;
using Explorer.Stakeholders.Core.Domain;
using Explorer.Stakeholders.Infrastructure.Database;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;

namespace Explorer.Stakeholders.Tests.Integration.WelcomeBonus;

[Collection("Sequential")]
public class WelcomeBonusCommandTests : BaseStakeholdersIntegrationTest
{
    public WelcomeBonusCommandTests(StakeholdersTestFactory factory) : base(factory) { }

    [Fact]
    public void Create_welcome_bonus_succeeds()
    {
        // Arrange
        using var scope = Factory.Services.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<IWelcomeBonusService>();
        var dbContext = scope.ServiceProvider.GetRequiredService<StakeholdersContext>();
        
        // Create a new person for this test to avoid conflicts
        var user = new User("newbonususer@test.com", "test123", UserRole.Tourist, true);
        dbContext.Users.Add(user);
        dbContext.SaveChanges();

        var person = new Person(user.Id, "New", "BonusUser", "newbonususer@test.com");
        dbContext.People.Add(person);
        dbContext.SaveChanges();

        var wallet = new Core.Domain.Wallet(person.Id);
        dbContext.Wallets.Add(wallet);
        dbContext.SaveChanges();

        // Act
        var bonus = service.CreateWelcomeBonus(person.Id);

        // Assert
        bonus.ShouldNotBeNull();
        bonus.PersonId.ShouldBe(person.Id);
        bonus.IsUsed.ShouldBeFalse();
        bonus.Value.ShouldBeGreaterThan(0);
        
        // Verify bonus type is valid (1-6)
        bonus.BonusType.ShouldBeInRange(1, 6);

        // Verify in database
        dbContext.ChangeTracker.Clear();
        var storedBonus = dbContext.WelcomeBonuses.FirstOrDefault(wb => wb.PersonId == person.Id);
        storedBonus.ShouldNotBeNull();
    }

    [Fact]
    public void Create_welcome_bonus_returns_existing_if_already_exists()
    {
        // Arrange
        using var scope = Factory.Services.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<IWelcomeBonusService>();
        
        // Tourist -21 već ima bonus u test data
        var personId = -21L;

        // Act
        var bonus = service.CreateWelcomeBonus(personId);

        // Assert - should return existing bonus, not create new
        bonus.ShouldNotBeNull();
        bonus.PersonId.ShouldBe(personId);
        bonus.BonusType.ShouldBe(1); // AC100 from test data
        bonus.Value.ShouldBe(100);
        bonus.IsUsed.ShouldBeTrue(); // Already used in test data
    }

    [Fact]
    public void AC_bonus_adds_coins_to_wallet()
    {
        // Arrange
        using var scope = Factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<StakeholdersContext>();
        
        // Create a new person and wallet for this test
        var user = new User("testbonus@test.com", "test123", UserRole.Tourist, true);
        dbContext.Users.Add(user);
        dbContext.SaveChanges();

        var person = new Person(user.Id, "Test", "Bonus", "testbonus@test.com");
        dbContext.People.Add(person);
        dbContext.SaveChanges();

        var wallet = new Core.Domain.Wallet(person.Id);
        dbContext.Wallets.Add(wallet);
        dbContext.SaveChanges();

        var initialBalance = wallet.BalanceAc;

        var service = scope.ServiceProvider.GetRequiredService<IWelcomeBonusService>();

        // Act
        var bonus = service.CreateWelcomeBonus(person.Id);

        // Assert
        dbContext.ChangeTracker.Clear();
        var updatedWallet = dbContext.Wallets.FirstOrDefault(w => w.PersonId == person.Id);
        updatedWallet.ShouldNotBeNull();

        // If it's an AC bonus, wallet should have been updated
        if (bonus.BonusType >= 1 && bonus.BonusType <= 3)
        {
            updatedWallet.BalanceAc.ShouldBe(initialBalance + bonus.Value);
        }
        else
        {
            // Discount bonus - wallet unchanged
            updatedWallet.BalanceAc.ShouldBe(initialBalance);
        }
    }

    [Fact]
    public void Get_active_discount_bonus_returns_null_for_used_bonus()
    {
        // Arrange
        using var scope = Factory.Services.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<IInternalWelcomeBonusService>();
        
        // Tourist -23 has used discount bonus
        var personId = -23L;

        // Act
        var bonus = service.GetActiveDiscountBonus(personId);

        // Assert
        bonus.ShouldBeNull();
    }

    [Fact]
    public void Get_active_discount_bonus_returns_bonus_for_active_discount()
    {
        // Arrange
        using var scope = Factory.Services.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<IInternalWelcomeBonusService>();
        
        // Tourist -22 has active discount bonus
        var personId = -22L;

        // Act
        var bonus = service.GetActiveDiscountBonus(personId);

        // Assert
        bonus.ShouldNotBeNull();
        bonus.PersonId.ShouldBe(personId);
        bonus.Value.ShouldBe(10);
        bonus.IsUsed.ShouldBeFalse();
    }

    [Fact]
    public void Get_active_discount_bonus_returns_null_for_ac_bonus()
    {
        // Arrange
        using var scope = Factory.Services.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<IInternalWelcomeBonusService>();
        
        // Tourist -21 has AC bonus (not discount)
        var personId = -21L;

        // Act
        var bonus = service.GetActiveDiscountBonus(personId);

        // Assert
        bonus.ShouldBeNull(); // AC bonus is not a discount bonus
    }

    [Fact]
    public void Mark_bonus_as_used_succeeds()
    {
        // Arrange
        using var scope = Factory.Services.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<IInternalWelcomeBonusService>();
        var dbContext = scope.ServiceProvider.GetRequiredService<StakeholdersContext>();
        
        // Tourist -22 has active discount bonus
        var personId = -22L;

        // Act
        service.MarkBonusAsUsed(personId);

        // Assert
        dbContext.ChangeTracker.Clear();
        var bonus = dbContext.WelcomeBonuses.FirstOrDefault(wb => wb.PersonId == personId);
        bonus.ShouldNotBeNull();
        bonus.IsUsed.ShouldBeTrue();
        bonus.UsedAt.ShouldNotBeNull();
    }
}
