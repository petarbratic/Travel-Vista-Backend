using Explorer.Encounters.API.Dtos;
using Explorer.Encounters.API.Public;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Xunit;

namespace Explorer.Encounters.Tests.Integration;

[Collection("Sequential")]
public class EncounterValidationTests : BaseEncountersIntegrationTest
{
    public EncounterValidationTests(EncountersTestFactory factory) : base(factory)
    {
    }

    [Fact]
    public void Create_Fails_WhenMiscMissingActionDescription()
    {
        // Arrange
        using var scope = Factory.Services.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<IEncounterService>();

        var dto = new EncounterDto
        {
            Name = "Misc Without Action",
            Description = "Missing action description",
            Latitude = 45.0,
            Longitude = 19.0,
            XP = 50,
            Type = "Misc",
            Status = "Draft",
            ActionDescription = null // Missing!
        };

        // Act & Assert
        Should.Throw<ArgumentException>(() => service.Create(dto))
            .Message.ShouldContain("ActionDescription");
    }

    [Fact]
    public void Create_Fails_WhenSocialMissingRequiredPeopleCount()
    {
        // Arrange
        using var scope = Factory.Services.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<IEncounterService>();

        var dto = new EncounterDto
        {
            Name = "Social Without People Count",
            Description = "Missing people count",
            Latitude = 45.0,
            Longitude = 19.0,
            XP = 80,
            Type = "Social",
            Status = "Draft",
            RequiredPeopleCount = null, // Missing!
            RangeInMeters = 20.0
        };

        // Act & Assert
        Should.Throw<ArgumentException>(() => service.Create(dto))
            .Message.ShouldContain("RequiredPeopleCount");
    }

    [Fact]
    public void Create_Fails_WhenSocialMissingRangeInMeters()
    {
        // Arrange
        using var scope = Factory.Services.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<IEncounterService>();

        var dto = new EncounterDto
        {
            Name = "Social Without Range",
            Description = "Missing range",
            Latitude = 45.0,
            Longitude = 19.0,
            XP = 80,
            Type = "Social",
            Status = "Draft",
            RequiredPeopleCount = 5,
            RangeInMeters = null // Missing!
        };

        // Act & Assert
        Should.Throw<ArgumentException>(() => service.Create(dto))
            .Message.ShouldContain("RangeInMeters");
    }

    [Fact]
    public void Create_Fails_WhenHiddenLocationMissingImageUrl()
    {
        // Arrange
        using var scope = Factory.Services.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<IEncounterService>();

        var dto = new EncounterDto
        {
            Name = "Hidden Without Image",
            Description = "Missing image",
            Latitude = 45.0,
            Longitude = 19.0,
            XP = 100,
            Type = "HiddenLocation",
            Status = "Draft",
            ImageUrl = null // Missing!
        };

        // Act & Assert
        Should.Throw<ArgumentException>(() => service.Create(dto))
            .Message.ShouldContain("ImageUrl");
    }

    [Fact]
    public void CanTouristCreateEncounter_ReturnsTrue()
    {
        // Arrange
        using var scope = Factory.Services.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<IEncounterService>();

        // Act
        var result = service.CanTouristCreateEncounter(-21);

        // Assert
        result.ShouldBeTrue();
    }
}