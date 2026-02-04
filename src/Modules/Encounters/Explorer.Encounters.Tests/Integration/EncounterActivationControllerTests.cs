using Explorer.API.Controllers.Tourist;
using Explorer.Encounters.API.Dtos;
using Explorer.Encounters.API.Public;
using Explorer.Tours.API.Dtos;
using Explorer.Tours.API.Public.Tourist;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Xunit;

namespace Explorer.Encounters.Tests.Integration;

[Collection("Sequential")]
public class EncounterActivationControllerTests : BaseEncountersIntegrationTest
{
    public EncounterActivationControllerTests(EncountersTestFactory factory) : base(factory)
    {
    }

    [Fact]
    public void GetNearbyEncounters_Success()
    {
        // Arrange
        using var scope = Factory.Services.CreateScope();
        var controller = CreateActivationController(scope);
        var positionService = scope.ServiceProvider.GetRequiredService<IPositionService>();

        long touristId = -21;

        // Postavi poziciju
        positionService.Update(touristId, new PositionDto
        {
            TouristId = touristId,
            Latitude = 45.2517,
            Longitude = 19.8658
        });

        // Act
        var result = ((ObjectResult)controller.GetNearbyEncounters(100).Result)?.Value as List<NearbyEncounterDto>;

        // Assert
        result.ShouldNotBeNull();
        result.Count.ShouldBeGreaterThan(0);
    }

    [Fact]
    public void GetNearbyEncounters_ReturnsBadRequest_WhenPositionNotSet()
    {
        // Arrange
        using var scope = Factory.Services.CreateScope();
        var controller = CreateActivationController(scope, "-99"); // Nepostojeći turista

        // Act
        var result = controller.GetNearbyEncounters(100).Result;

        // Assert
        result.ShouldBeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public void ActivateEncounter_Success()
    {
        // Arrange
        using var scope = Factory.Services.CreateScope();
        var controller = CreateActivationController(scope);
        var positionService = scope.ServiceProvider.GetRequiredService<IPositionService>();
        var encounterService = scope.ServiceProvider.GetRequiredService<IEncounterService>();

        long touristId = -21;

        // Kreiraj aktivan encounter
        var encounter = encounterService.Create(new EncounterDto
        {
            Name = "Controller Activate Test",
            Description = "Test",
            Latitude = 45.0,
            Longitude = 19.0,
            XP = 50,
            Type = "Misc",
            Status = "Active",
            ActionDescription = "Do something"
        });

        // Postavi poziciju blizu
        positionService.Update(touristId, new PositionDto
        {
            TouristId = touristId,
            Latitude = 45.0,
            Longitude = 19.0
        });

        // Act
        var result = ((ObjectResult)controller.ActivateEncounter(encounter.Id).Result)?.Value as EncounterActivationDto;

        // Assert
        result.ShouldNotBeNull();
        result.EncounterId.ShouldBe(encounter.Id);
        result.Status.ShouldBe("InProgress");
    }

    [Fact]
    public void ActivateEncounter_ReturnsNotFound_WhenEncounterDoesNotExist()
    {
        // Arrange
        using var scope = Factory.Services.CreateScope();
        var controller = CreateActivationController(scope);

        // Act
        var result = controller.ActivateEncounter(-9999).Result;

        // Assert
        result.ShouldBeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public void ActivateEncounter_ReturnsBadRequest_WhenTooFar()
    {
        // Arrange
        using var scope = Factory.Services.CreateScope();
        var controller = CreateActivationController(scope);
        var positionService = scope.ServiceProvider.GetRequiredService<IPositionService>();
        var encounterService = scope.ServiceProvider.GetRequiredService<IEncounterService>();

        long touristId = -21;

        var encounter = encounterService.Create(new EncounterDto
        {
            Name = "Far Encounter",
            Description = "Far away",
            Latitude = 44.0,
            Longitude = 18.0,
            XP = 50,
            Type = "Misc",
            Status = "Active",
            ActionDescription = "Action"
        });

        // Postavi poziciju DALEKO
        positionService.Update(touristId, new PositionDto
        {
            TouristId = touristId,
            Latitude = 45.5,
            Longitude = 19.5
        });

        // Act
        var result = controller.ActivateEncounter(encounter.Id).Result;

        // Assert
        result.ShouldBeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public void GetActiveEncounters_ReturnsOnlyInProgressActivations()
    {
        // Arrange
        using var scope = Factory.Services.CreateScope();
        var controller = CreateActivationController(scope);

        // Act
        var result = ((ObjectResult)controller.GetActiveEncounters().Result)?.Value as List<EncounterActivationDto>;

        // Assert
        result.ShouldNotBeNull();
        result.ShouldAllBe(a => a.Status == "InProgress");
    }

    [Fact]
    public void CompleteEncounter_Success()
    {
        // Arrange
        using var scope = Factory.Services.CreateScope();
        var controller = CreateActivationController(scope);
        var positionService = scope.ServiceProvider.GetRequiredService<IPositionService>();
        var encounterService = scope.ServiceProvider.GetRequiredService<IEncounterService>();

        long touristId = -21;

        // Kreiraj i aktiviraj encounter
        var encounter = encounterService.Create(new EncounterDto
        {
            Name = "Complete Test Controller",
            Description = "To be completed",
            Latitude = 45.0,
            Longitude = 19.0,
            XP = 50,
            Type = "Misc",
            Status = "Active",
            ActionDescription = "Complete this"
        });

        positionService.Update(touristId, new PositionDto
        {
            TouristId = touristId,
            Latitude = 45.0,
            Longitude = 19.0
        });

        var activationService = scope.ServiceProvider.GetRequiredService<IEncounterActivationService>();
        activationService.ActivateEncounter(touristId, encounter.Id);

        // Act
        var result = ((ObjectResult)controller.CompleteEncounter(encounter.Id).Result)?.Value as EncounterActivationDto;

        // Assert
        result.ShouldNotBeNull();
        result.Status.ShouldBe("Completed");
        result.CompletedAt.ShouldNotBeNull();
    }

    [Fact]
    public void CompleteEncounter_ReturnsBadRequest_WhenNotActive()
    {
        // Arrange
        using var scope = Factory.Services.CreateScope();
        var controller = CreateActivationController(scope);

        // Act
        var result = controller.CompleteEncounter(-9999).Result;

        // Assert
        result.ShouldBeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public void AbandonEncounter_Success()
    {
        // Arrange
        using var scope = Factory.Services.CreateScope();
        var controller = CreateActivationController(scope, "-21");
        var positionService = scope.ServiceProvider.GetRequiredService<IPositionService>();
        var encounterService = scope.ServiceProvider.GetRequiredService<IEncounterService>();

        long touristId = -21;

        // Kreiraj i aktiviraj encounter
        var encounter = encounterService.Create(new EncounterDto
        {
            Name = "Abandon Test Controller",
            Description = "To be abandoned",
            Latitude = 45.0,
            Longitude = 19.0,
            XP = 50,
            Type = "Misc",
            Status = "Active",
            ActionDescription = "Abandon this"
        });

        positionService.Update(touristId, new PositionDto
        {
            TouristId = touristId,
            Latitude = 45.0,
            Longitude = 19.0
        });

        var activationService = scope.ServiceProvider.GetRequiredService<IEncounterActivationService>();
        activationService.ActivateEncounter(touristId, encounter.Id);

        // Act
        var result = ((ObjectResult)controller.AbandonEncounter(encounter.Id).Result)?.Value as EncounterActivationDto;

        // Assert
        result.ShouldNotBeNull();
        result.Status.ShouldBe("Failed");
    }

    [Fact]
    public void AbandonEncounter_ReturnsBadRequest_WhenNotActive()
    {
        // Arrange
        using var scope = Factory.Services.CreateScope();
        var controller = CreateActivationController(scope);

        // Act
        var result = controller.AbandonEncounter(-9999).Result;

        // Assert
        result.ShouldBeOfType<BadRequestObjectResult>();
    }

    private static EncounterActivationController CreateActivationController(IServiceScope scope, string touristId = "-21")
    {
        return new EncounterActivationController(
            scope.ServiceProvider.GetRequiredService<IEncounterActivationService>())
        {
            ControllerContext = BuildContext(touristId)
        };
    }
}