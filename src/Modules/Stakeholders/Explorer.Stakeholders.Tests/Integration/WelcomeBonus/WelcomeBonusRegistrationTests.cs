using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Explorer.API.Controllers;
using Explorer.Stakeholders.API.Dtos;
using Explorer.Stakeholders.API.Public;
using Explorer.Stakeholders.Infrastructure.Database;

namespace Explorer.Stakeholders.Tests.Integration.WelcomeBonus;

[Collection("Sequential")]
public class WelcomeBonusRegistrationTests : BaseStakeholdersIntegrationTest
{
    public WelcomeBonusRegistrationTests(StakeholdersTestFactory factory) : base(factory) { }

    [Fact]
    public void Registration_creates_welcome_bonus()
    {
        // Arrange
        using var scope = Factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<StakeholdersContext>();
        var controller = CreateController(scope);
        var account = new AccountRegistrationDto
        {
            Username = "bonustest@gmail.com",
            Email = "bonustest@gmail.com",
            Password = "bonustest",
            Name = "Bonus",
            Surname = "Tester"
        };

        // Act
        var authResult = ((ObjectResult)controller.RegisterTourist(account).Result).Value as AuthenticationTokensDto;

        // Assert - Registration succeeded
        authResult.ShouldNotBeNull();
        var decodedToken = new JwtSecurityTokenHandler().ReadJwtToken(authResult.AccessToken);
        var personIdClaim = decodedToken.Claims.FirstOrDefault(c => c.Type == "personId");
        personIdClaim.ShouldNotBeNull();
        var personId = long.Parse(personIdClaim.Value);

        // Assert - Welcome bonus was created
        dbContext.ChangeTracker.Clear();
        var bonus = dbContext.WelcomeBonuses.FirstOrDefault(wb => wb.PersonId == personId);
        bonus.ShouldNotBeNull();
        bonus.IsUsed.ShouldBeFalse();
        bonus.Value.ShouldBeGreaterThan(0);
    }

    [Fact]
    public void Registration_creates_wallet_with_ac_if_ac_bonus()
    {
        // Arrange
        using var scope = Factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<StakeholdersContext>();
        var controller = CreateController(scope);
        var account = new AccountRegistrationDto
        {
            Username = "wallettest@gmail.com",
            Email = "wallettest@gmail.com",
            Password = "wallettest",
            Name = "Wallet",
            Surname = "Tester"
        };

        // Act
        var authResult = ((ObjectResult)controller.RegisterTourist(account).Result).Value as AuthenticationTokensDto;

        // Assert
        authResult.ShouldNotBeNull();
        var decodedToken = new JwtSecurityTokenHandler().ReadJwtToken(authResult.AccessToken);
        var personIdClaim = decodedToken.Claims.FirstOrDefault(c => c.Type == "personId");
        var personId = long.Parse(personIdClaim!.Value);

        dbContext.ChangeTracker.Clear();
        var bonus = dbContext.WelcomeBonuses.FirstOrDefault(wb => wb.PersonId == personId);
        var wallet = dbContext.Wallets.FirstOrDefault(w => w.PersonId == personId);

        bonus.ShouldNotBeNull();
        wallet.ShouldNotBeNull();

        // If AC bonus, wallet should have the AC value
        // BonusType 1-3 are AC bonuses
        if ((int)bonus.BonusType >= 1 && (int)bonus.BonusType <= 3)
        {
            wallet.BalanceAc.ShouldBe(bonus.Value);
        }
        else
        {
            // Discount bonus - wallet starts at 0
            wallet.BalanceAc.ShouldBe(0);
        }
    }

    private static AuthenticationController CreateController(IServiceScope scope)
    {
        return new AuthenticationController(scope.ServiceProvider.GetRequiredService<IAuthenticationService>());
    }
}
