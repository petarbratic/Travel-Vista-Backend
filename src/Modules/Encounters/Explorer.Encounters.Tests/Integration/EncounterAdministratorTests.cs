using Explorer.Encounters.API.Dtos;
using Explorer.Encounters.API.Public;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Xunit;

namespace Explorer.Encounters.Tests.Integration;

[Collection("Sequential")]
public class EncounterAdministratorTests : BaseEncountersIntegrationTest
{
    public EncounterAdministratorTests(EncountersTestFactory factory) : base(factory)
    {
    }

    [Fact]
    public void GetAll_ReturnsAllEncounters()
    {
        // Arrange
        using var scope = Factory.Services.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<IEncounterService>();

        // Act
        var result = service.GetAll();

        // Assert
        result.ShouldNotBeNull();
        result.Count.ShouldBeGreaterThan(0);
    }

    [Fact]
    public void Get_Success()
    {
        // Arrange
        using var scope = Factory.Services.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<IEncounterService>();

        var created = service.Create(new EncounterDto
        {
            Name = "Test Get",
            Description = "Test",
            Latitude = 45.0,
            Longitude = 19.0,
            XP = 50,
            Type = "Misc",
            Status = "Draft",
            ActionDescription = "Do something"
        });

        // Act
        var result = service.Get(created.Id);

        // Assert
        result.ShouldNotBeNull();
        result.Id.ShouldBe(created.Id);
        result.Name.ShouldBe("Test Get");
    }

    [Fact]
    public void Get_Fails_WhenNotFound()
    {
        // Arrange
        using var scope = Factory.Services.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<IEncounterService>();

        // Act & Assert
        Should.Throw<KeyNotFoundException>(() => service.Get(-9999));
    }

    [Fact]
    public void Approve_Success()
    {
        // Arrange
        using var scope = Factory.Services.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<IEncounterService>();

        var created = service.Create(new EncounterDto
        {
            Name = "Pending Approval",
            Description = "Waiting for admin approval",
            Latitude = 45.0,
            Longitude = 19.0,
            XP = 100,
            Type = "HiddenLocation",
            Status = "PendingApproval",
            ImageUrl = "https://example.com/image.jpg"
        });

        // Act
        var result = service.Approve(created.Id);

        // Assert
        result.ShouldNotBeNull();
        result.Status.ShouldBe("Active");
    }

    [Fact]
    public void Approve_Fails_WhenNotFound()
    {
        // Arrange
        using var scope = Factory.Services.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<IEncounterService>();

        // Act & Assert
        Should.Throw<KeyNotFoundException>(() => service.Approve(-9999));
    }

    [Fact]
    public void Reject_Success()
    {
        // Arrange
        using var scope = Factory.Services.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<IEncounterService>();

        var created = service.Create(new EncounterDto
        {
            Name = "To Be Rejected",
            Description = "This will be rejected",
            Latitude = 45.0,
            Longitude = 19.0,
            XP = 50,
            Type = "Social",
            Status = "PendingApproval",
            RequiredPeopleCount = 3,
            RangeInMeters = 20.0
        });

        // Act
        var result = service.Reject(created.Id);

        // Assert
        result.ShouldNotBeNull();
        result.Status.ShouldBe("Archived");
    }

    [Fact]
    public void Reject_Fails_WhenNotFound()
    {
        // Arrange
        using var scope = Factory.Services.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<IEncounterService>();

        // Act & Assert
        Should.Throw<KeyNotFoundException>(() => service.Reject(-9999));
    }

    [Fact]
    public void Update_Fails_WhenNotFound()
    {
        // Arrange
        using var scope = Factory.Services.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<IEncounterService>();

        var dto = new EncounterDto
        {
            Id = -9999,
            Name = "Nonexistent",
            Description = "Does not exist",
            Latitude = 45.0,
            Longitude = 19.0,
            XP = 50,
            Type = "Misc",
            Status = "Draft",
            ActionDescription = "Action"
        };

        // Act & Assert
        Should.Throw<KeyNotFoundException>(() => service.Update(dto));
    }
}