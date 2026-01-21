using Explorer.API.Controllers.Tourist;
using Explorer.Stakeholders.API.Dtos;
using Explorer.Stakeholders.API.Public;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using System.Security.Claims;

namespace Explorer.Stakeholders.Tests.Integration.WelcomeBonus;

[Collection("Sequential")]
public class WelcomeBonusQueryTests : BaseStakeholdersIntegrationTest
{
    public WelcomeBonusQueryTests(StakeholdersTestFactory factory) : base(factory) { }

    [Fact]
    public void Get_welcome_bonus_succeeds_for_tourist_with_bonus()
    {
        // Arrange
        using var scope = Factory.Services.CreateScope();
        var controller = CreateController(scope, -21); // Tourist sa AC bonusom

        // Act
        var result = controller.GetWelcomeBonus();
        var okResult = result.Result as OkObjectResult;

        // Assert
        okResult.ShouldNotBeNull();
        var bonus = okResult.Value as WelcomeBonusDto;
        bonus.ShouldNotBeNull();
        bonus.PersonId.ShouldBe(-21);
        bonus.BonusType.ShouldBe(1); // AC100
        bonus.Value.ShouldBe(100);
        bonus.IsUsed.ShouldBeTrue();
    }

    [Fact]
    public void Get_welcome_bonus_returns_active_discount_bonus()
    {
        // Arrange
        using var scope = Factory.Services.CreateScope();
        var controller = CreateController(scope, -22); // Tourist sa aktivnim popustom

        // Act
        var result = controller.GetWelcomeBonus();
        var okResult = result.Result as OkObjectResult;

        // Assert
        okResult.ShouldNotBeNull();
        var bonus = okResult.Value as WelcomeBonusDto;
        bonus.ShouldNotBeNull();
        bonus.PersonId.ShouldBe(-22);
        bonus.BonusType.ShouldBe(4); // Discount10
        bonus.Value.ShouldBe(10);
        bonus.IsUsed.ShouldBeFalse();
    }

    [Fact]
    public void Get_welcome_bonus_returns_not_found_for_nonexistent_person()
    {
        // Arrange
        using var scope = Factory.Services.CreateScope();
        var controller = CreateController(scope, -9999); // Person that doesn't exist

        // Act
        var result = controller.GetWelcomeBonus();

        // Assert
        result.Result.ShouldBeOfType<NotFoundObjectResult>();
    }

    private static WelcomeBonusController CreateController(IServiceScope scope, long personId)
    {
        var service = scope.ServiceProvider.GetRequiredService<IWelcomeBonusService>();
        return new WelcomeBonusController(service)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity(new[]
                    {
                        new Claim("personId", personId.ToString())
                    }, "TestAuth"))
                }
            }
        };
    }
}
