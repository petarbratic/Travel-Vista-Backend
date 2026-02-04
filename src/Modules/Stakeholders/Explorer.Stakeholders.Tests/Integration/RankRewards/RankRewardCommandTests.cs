using Explorer.API.Controllers.Tourist;
using Explorer.Stakeholders.API.Dtos;
using Explorer.Stakeholders.API.Public;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using System.Security.Claims;
using Xunit;

namespace Explorer.Stakeholders.Tests.Integration.RankRewards;

[Collection("Sequential")]
public class RankRewardCommandTests : BaseStakeholdersIntegrationTest
{
    public RankRewardCommandTests(StakeholdersTestFactory factory) : base(factory) { }

    private static ClaimsPrincipal BuildUser(long personId)
    {
        var claims = new[] { new Claim("personId", personId.ToString()) };
        return new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuth"));
    }

    private static ControllerContext BuildContext(long personId)
        => new ControllerContext { HttpContext = new DefaultHttpContext { User = BuildUser(personId) } };

    private static RankRewardController CreateController(IServiceScope scope, long touristPersonId)
    {
        var rankRewardService = scope.ServiceProvider.GetRequiredService<IRankRewardService>();

        return new RankRewardController(rankRewardService)
        {
            ControllerContext = BuildContext(touristPersonId)
        };
    }

    [Fact]
    public void Rookie_tourist_cannot_claim_any_rewards()
    {
        // Arrange: Tourist -24 is Rookie (Level 1, XP 0)
        using var scope = Factory.Services.CreateScope();
        var controller = CreateController(scope, -24);

        // Act
        var actionResult = controller.ClaimRewards();
        var badRequest = actionResult.Result as BadRequestObjectResult;

        // Assert
        badRequest.ShouldNotBeNull();
        var result = badRequest!.Value as RankRewardClaimResultDto;
        result.ShouldNotBeNull();
        result!.Success.ShouldBeFalse();
        result.Message.ShouldBe("No unclaimed rank rewards available.");
        result.AcAwarded.ShouldBe(0);
        result.ClaimedRanks.Count.ShouldBe(0);
    }

    [Fact]
    public void Bronze_tourist_cannot_claim_any_rewards()
    {
        // Arrange: Tourist -21 is Bronze (Level 3, XP 200)
        using var scope = Factory.Services.CreateScope();
        var controller = CreateController(scope, -21);

        // Act
        var actionResult = controller.ClaimRewards();
        var badRequest = actionResult.Result as BadRequestObjectResult;

        // Assert
        badRequest.ShouldNotBeNull();
        var result = badRequest!.Value as RankRewardClaimResultDto;
        result.ShouldNotBeNull();
        result!.Success.ShouldBeFalse();
        result.AcAwarded.ShouldBe(0);
    }

    [Fact]
    public void Gold_tourist_can_claim_500_ac_reward()
    {
        // Arrange: Tourist -23 is Gold (Level 12, XP 1100)
        using var scope = Factory.Services.CreateScope();
        var controller = CreateController(scope, -23);
        var walletService = scope.ServiceProvider.GetRequiredService<IWalletService>();

        var walletBefore = walletService.GetMyWallet(-23);
        var balanceBefore = walletBefore.BalanceAc;

        // Act
        var actionResult = controller.ClaimRewards();
        var ok = actionResult.Result as OkObjectResult;

        // Assert
        ok.ShouldNotBeNull();
        var result = ok!.Value as RankRewardClaimResultDto;
        result.ShouldNotBeNull();
        result!.Success.ShouldBeTrue();
        result.AcAwarded.ShouldBe(500);
        result.ClaimedRanks.ShouldContain("Gold");
        result.ClaimedRanks.Count.ShouldBe(1);

        // Verify wallet was updated
        var walletAfter = walletService.GetMyWallet(-23);
        walletAfter.BalanceAc.ShouldBe(balanceBefore + 500);
    }

    [Fact]
    public void Diamond_tourist_can_claim_multiple_rewards_at_once()
    {
        // Arrange: Tourist -22 is Diamond (Level 25, XP 2400)
        // Should be able to claim: Gold (500) + Platinum (500) + Diamond (1000) = 2000 AC
        using var scope = Factory.Services.CreateScope();
        var controller = CreateController(scope, -22);
        var walletService = scope.ServiceProvider.GetRequiredService<IWalletService>();

        var walletBefore = walletService.GetMyWallet(-22);
        var balanceBefore = walletBefore.BalanceAc;

        // Act
        var actionResult = controller.ClaimRewards();
        var ok = actionResult.Result as OkObjectResult;

        // Assert
        ok.ShouldNotBeNull();
        var result = ok!.Value as RankRewardClaimResultDto;
        result.ShouldNotBeNull();
        result!.Success.ShouldBeTrue();
        result.AcAwarded.ShouldBe(2000);
        result.ClaimedRanks.ShouldContain("Gold");
        result.ClaimedRanks.ShouldContain("Platinum");
        result.ClaimedRanks.ShouldContain("Diamond");
        result.ClaimedRanks.Count.ShouldBe(3);

        // Verify wallet was updated
        var walletAfter = walletService.GetMyWallet(-22);
        walletAfter.BalanceAc.ShouldBe(balanceBefore + 2000);
    }

    [Fact]
    public void Vista_tourist_can_claim_all_rewards()
    {
        // Arrange: Tourist -25 is Vista (Level 35, XP 3400)
        // Should be able to claim: Gold (500) + Platinum (500) + Diamond (1000) + Vista (2000) = 4000 AC
        using var scope = Factory.Services.CreateScope();
        var controller = CreateController(scope, -25);
        var walletService = scope.ServiceProvider.GetRequiredService<IWalletService>();

        var walletBefore = walletService.GetMyWallet(-25);
        var balanceBefore = walletBefore.BalanceAc;

        // Act
        var actionResult = controller.ClaimRewards();
        var ok = actionResult.Result as OkObjectResult;

        // Assert
        ok.ShouldNotBeNull();
        var result = ok!.Value as RankRewardClaimResultDto;
        result.ShouldNotBeNull();
        result!.Success.ShouldBeTrue();
        result.AcAwarded.ShouldBe(4000);
        result.ClaimedRanks.ShouldContain("Gold");
        result.ClaimedRanks.ShouldContain("Platinum");
        result.ClaimedRanks.ShouldContain("Diamond");
        result.ClaimedRanks.ShouldContain("Vista");
        result.ClaimedRanks.Count.ShouldBe(4);

        // Verify wallet was updated
        var walletAfter = walletService.GetMyWallet(-25);
        walletAfter.BalanceAc.ShouldBe(balanceBefore + 4000);
    }

    [Fact]
    public void Cannot_claim_same_reward_twice()
    {
        // Arrange: Tourist -26 has already claimed Gold reward (Level 13, XP 1200)
        using var scope = Factory.Services.CreateScope();
        var controller = CreateController(scope, -26);

        // Act: First attempt should fail (already claimed)
        var firstAttempt = controller.ClaimRewards();
        var badRequest1 = firstAttempt.Result as BadRequestObjectResult;

        // Assert
        badRequest1.ShouldNotBeNull();
        var result1 = badRequest1!.Value as RankRewardClaimResultDto;
        result1.ShouldNotBeNull();
        result1!.Success.ShouldBeFalse();
        result1.Message.ShouldBe("No unclaimed rank rewards available.");
        result1.AcAwarded.ShouldBe(0);
    }

    [Fact]
    public void Claiming_rewards_updates_tracking_properly()
    {
        // Arrange: Tourist -27 is Gold (Level 12) - UNIQUE turista za ovaj test
        using var scope = Factory.Services.CreateScope();
        var controller = CreateController(scope, -27);

        // Act: Claim Gold reward
        var firstClaim = controller.ClaimRewards();
        var ok1 = firstClaim.Result as OkObjectResult;
        ok1.ShouldNotBeNull();

        // Try to claim again - should fail
        var secondClaim = controller.ClaimRewards();
        var badRequest = secondClaim.Result as BadRequestObjectResult;

        // Assert
        badRequest.ShouldNotBeNull();
        var result = badRequest!.Value as RankRewardClaimResultDto;
        result.ShouldNotBeNull();
        result!.Success.ShouldBeFalse();
        result.Message.ShouldBe("No unclaimed rank rewards available.");
    }

    [Fact]
    public void Non_existent_tourist_throws_key_not_found()
    {
        // Arrange: Person -999 doesn't exist
        using var scope = Factory.Services.CreateScope();
        var controller = CreateController(scope, -999);

        // Act
        var actionResult = controller.ClaimRewards();
        var notFound = actionResult.Result as NotFoundObjectResult;

        // Assert
        notFound.ShouldNotBeNull();
        notFound.StatusCode.ShouldBe(404);
    }

    [Fact]
    public void Wallet_balance_persists_after_claiming()
    {
        // Arrange: Tourist -28 is Diamond - UNIQUE turista za ovaj test
        using var scope = Factory.Services.CreateScope();
        var controller = CreateController(scope, -28);
        var walletService = scope.ServiceProvider.GetRequiredService<IWalletService>();

        // Get initial balance
        var initialWallet = walletService.GetMyWallet(-28);
        var initialBalance = initialWallet.BalanceAc;

        // Act: Claim rewards
        controller.ClaimRewards();

        // Get balance again from a fresh scope to ensure persistence
        using var scope2 = Factory.Services.CreateScope();
        var walletService2 = scope2.ServiceProvider.GetRequiredService<IWalletService>();
        var finalWallet = walletService2.GetMyWallet(-28);

        // Assert: Balance should have increased by 2000 AC
        finalWallet.BalanceAc.ShouldBe(initialBalance + 2000);
    }
}