using Explorer.API.Controllers.Administrator;
using Explorer.Encounters.API.Dtos;
using Explorer.Encounters.API.Public;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Xunit;

namespace Explorer.Encounters.Tests.Integration;

[Collection("Sequential")]
public class EncounterAdministratorControllerTests : BaseEncountersIntegrationTest
{
    public EncounterAdministratorControllerTests(EncountersTestFactory factory) : base(factory)
    {
    }

    [Fact]
    public void GetAll_ReturnsAllEncounters()
    {
        // Arrange
        using var scope = Factory.Services.CreateScope();
        var controller = CreateAdminController(scope);

        // Act
        var result = ((ObjectResult)controller.GetAll().Result)?.Value as List<EncounterDto>;

        // Assert
        result.ShouldNotBeNull();
        result.Count.ShouldBeGreaterThan(0);
    }

    [Fact]
    public void Get_ReturnsEncounter()
    {
        // Arrange
        using var scope = Factory.Services.CreateScope();
        var controller = CreateAdminController(scope);

        // Prvo kreiraj encounter
        var createDto = new EncounterDto
        {
            Name = "Test Get Controller",
            Description = "Testing Get endpoint",
            Latitude = 45.0,
            Longitude = 19.0,
            XP = 50,
            Type = "Misc",
            ActionDescription = "Do something"
        };
        var created = ((ObjectResult)controller.Create(createDto).Result)?.Value as EncounterDto;

        // Act
        var result = ((ObjectResult)controller.Get(created.Id).Result)?.Value as EncounterDto;

        // Assert
        result.ShouldNotBeNull();
        result.Id.ShouldBe(created.Id);
        result.Name.ShouldBe("Test Get Controller");
    }

    [Fact]
    public void Get_ReturnsNotFound_WhenEncounterDoesNotExist()
    {
        // Arrange
        using var scope = Factory.Services.CreateScope();
        var controller = CreateAdminController(scope);

        // Act
        var result = controller.Get(-9999).Result;

        // Assert
        result.ShouldBeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public void Create_Success_SetsDraftStatus()
    {
        // Arrange
        using var scope = Factory.Services.CreateScope();
        var controller = CreateAdminController(scope);

        var dto = new EncounterDto
        {
            Name = "Admin Created",
            Description = "Created by admin",
            Latitude = 45.0,
            Longitude = 19.0,
            XP = 100,
            Type = "HiddenLocation",
            ImageUrl = "https://example.com/image.jpg",
            Status = "Active" // Admin pokušava da setuje Active
        };

        // Act
        var result = ((ObjectResult)controller.Create(dto).Result)?.Value as EncounterDto;

        // Assert
        result.ShouldNotBeNull();
        result.Status.ShouldBe("Draft"); // Kontroler forsira Draft
    }

    [Fact]
    public void Create_ReturnsBadRequest_WhenValidationFails()
    {
        // Arrange
        using var scope = Factory.Services.CreateScope();
        var controller = CreateAdminController(scope);

        var dto = new EncounterDto
        {
            Name = "Invalid Misc",
            Description = "Missing action",
            Latitude = 45.0,
            Longitude = 19.0,
            XP = 50,
            Type = "Misc",
            ActionDescription = null // Missing!
        };

        // Act
        var result = controller.Create(dto).Result;

        // Assert
        result.ShouldBeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public void Update_Success()
    {
        // Arrange
        using var scope = Factory.Services.CreateScope();
        var controller = CreateAdminController(scope);

        var createDto = new EncounterDto
        {
            Name = "Original Name",
            Description = "Original description",
            Latitude = 45.0,
            Longitude = 19.0,
            XP = 50,
            Type = "Social",
            RequiredPeopleCount = 3,
            RangeInMeters = 15.0
        };
        var created = ((ObjectResult)controller.Create(createDto).Result)?.Value as EncounterDto;

        // Act
        created.Name = "Updated Name";
        created.XP = 150;
        var result = ((ObjectResult)controller.Update(created.Id, created).Result)?.Value as EncounterDto;

        // Assert
        result.ShouldNotBeNull();
        result.Name.ShouldBe("Updated Name");
        result.XP.ShouldBe(150);
    }

    [Fact]
    public void Update_ReturnsNotFound_WhenEncounterDoesNotExist()
    {
        // Arrange
        using var scope = Factory.Services.CreateScope();
        var controller = CreateAdminController(scope);

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

        // Act
        var result = controller.Update(-9999, dto).Result;

        // Assert
        result.ShouldBeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public void Delete_Success()
    {
        // Arrange
        using var scope = Factory.Services.CreateScope();
        var controller = CreateAdminController(scope);

        var createDto = new EncounterDto
        {
            Name = "To Be Deleted Controller",
            Description = "Will be deleted",
            Latitude = 45.0,
            Longitude = 19.0,
            XP = 50,
            Type = "Misc",
            ActionDescription = "Simple action"
        };
        var created = ((ObjectResult)controller.Create(createDto).Result)?.Value as EncounterDto;

        // Act
        var result = controller.Delete(created.Id);

        // Assert
        result.ShouldBeOfType<NoContentResult>();
    }

    [Fact]
    public void Delete_ReturnsNotFound_WhenEncounterDoesNotExist()
    {
        // Arrange
        using var scope = Factory.Services.CreateScope();
        var controller = CreateAdminController(scope);

        // Act
        var result = controller.Delete(-9999);

        // Assert
        result.ShouldBeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public void Approve_Success_ChangesStatusToActive()
    {
        // Arrange
        using var scope = Factory.Services.CreateScope();
        var controller = CreateAdminController(scope);

        var createDto = new EncounterDto
        {
            Name = "Pending Approval Controller",
            Description = "Waiting for admin",
            Latitude = 45.0,
            Longitude = 19.0,
            XP = 100,
            Type = "HiddenLocation",
            ImageUrl = "https://example.com/image.jpg"
        };
        var created = ((ObjectResult)controller.Create(createDto).Result)?.Value as EncounterDto;

        // Act
        var result = ((ObjectResult)controller.Approve(created.Id).Result)?.Value as EncounterDto;

        // Assert
        result.ShouldNotBeNull();
        result.Status.ShouldBe("Active");
    }

    [Fact]
    public void Approve_ReturnsNotFound_WhenEncounterDoesNotExist()
    {
        // Arrange
        using var scope = Factory.Services.CreateScope();
        var controller = CreateAdminController(scope);

        // Act
        var result = controller.Approve(-9999).Result;

        // Assert
        result.ShouldBeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public void Reject_Success_ChangesStatusToArchived()
    {
        // Arrange
        using var scope = Factory.Services.CreateScope();
        var controller = CreateAdminController(scope);

        var createDto = new EncounterDto
        {
            Name = "To Be Rejected Controller",
            Description = "Will be rejected",
            Latitude = 45.0,
            Longitude = 19.0,
            XP = 50,
            Type = "Social",
            RequiredPeopleCount = 3,
            RangeInMeters = 20.0
        };
        var created = ((ObjectResult)controller.Create(createDto).Result)?.Value as EncounterDto;

        // Act
        var result = ((ObjectResult)controller.Reject(created.Id).Result)?.Value as EncounterDto;

        // Assert
        result.ShouldNotBeNull();
        result.Status.ShouldBe("Archived");
    }

    [Fact]
    public void Reject_ReturnsNotFound_WhenEncounterDoesNotExist()
    {
        // Arrange
        using var scope = Factory.Services.CreateScope();
        var controller = CreateAdminController(scope);

        // Act
        var result = controller.Reject(-9999).Result;

        // Assert
        result.ShouldBeOfType<NotFoundObjectResult>();
    }

    private static EncounterController CreateAdminController(IServiceScope scope)
    {
        return new EncounterController(scope.ServiceProvider.GetRequiredService<IEncounterService>())
        {
            ControllerContext = BuildContext("-11") // Admin person ID
        };
    }
}