using Explorer.API.Controllers.Tourist;
using Explorer.Encounters.API.Dtos;
using Explorer.Encounters.API.Public;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Xunit;

namespace Explorer.Encounters.Tests.Integration;

[Collection("Sequential")]
public class EncounterTouristControllerTests : BaseEncountersIntegrationTest
{
    public EncounterTouristControllerTests(EncountersTestFactory factory) : base(factory)
    {
    }

    [Fact]
    public void GetActiveEncounters_ReturnsOnlyActiveEncounters()
    {
        // Arrange
        using var scope = Factory.Services.CreateScope();
        var controller = CreateTouristController(scope);

        // Act
        var result = ((ObjectResult)controller.GetActiveEncounters().Result)?.Value as List<EncounterDto>;

        // Assert
        result.ShouldNotBeNull();
        result.ShouldAllBe(e => e.Status == "Active");
    }

    [Fact]
    public void Create_Success_SetsPendingApprovalStatus()
    {
        // Arrange
        using var scope = Factory.Services.CreateScope();
        var controller = CreateTouristController(scope);

        var dto = new EncounterDto
        {
            Name = "Tourist Created",
            Description = "Created by tourist",
            Latitude = 45.0,
            Longitude = 19.0,
            XP = 100,
            Type = "HiddenLocation",
            ImageUrl = "https://example.com/image.jpg",
            Status = "Active" // Turista pokušava da setuje Active
        };

        // Act
        var result = ((ObjectResult)controller.Create(dto).Result)?.Value as EncounterDto;

        // Assert
        result.ShouldNotBeNull();
        result.Status.ShouldBe("PendingApproval"); // Kontroler forsira PendingApproval
    }

    [Fact]
    public void Create_ReturnsBadRequest_WhenValidationFails()
    {
        // Arrange
        using var scope = Factory.Services.CreateScope();
        var controller = CreateTouristController(scope);

        var dto = new EncounterDto
        {
            Name = "Invalid Social",
            Description = "Missing required fields",
            Latitude = 45.0,
            Longitude = 19.0,
            XP = 80,
            Type = "Social",
            RequiredPeopleCount = null, // Missing!
            RangeInMeters = 20.0
        };

        // Act
        var result = controller.Create(dto).Result;

        // Assert
        result.ShouldBeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public void CanCreate_ReturnsTrue()
    {
        // Arrange
        using var scope = Factory.Services.CreateScope();
        var controller = CreateTouristController(scope);

        // Act
        var result = ((ObjectResult)controller.CanCreate().Result)?.Value as bool?;

        // Assert
        result.ShouldNotBeNull();
        result.Value.ShouldBeTrue();
    }

    private static Explorer.API.Controllers.Tourist.EncounterController CreateTouristController(IServiceScope scope)
    {
        return new Explorer.API.Controllers.Tourist.EncounterController(
            scope.ServiceProvider.GetRequiredService<IEncounterService>())
        {
            ControllerContext = BuildContext("-21") // Tourist person ID
        };
    }
}