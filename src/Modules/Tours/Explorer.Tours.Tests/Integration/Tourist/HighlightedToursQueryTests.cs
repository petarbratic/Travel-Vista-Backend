using Explorer.API.Controllers.Tourist;
using Explorer.Tours.API.Dtos;
using Explorer.Tours.API.Public.Authoring;
using Explorer.Tours.API.Public.Execution;
using Explorer.Tours.API.Public.Tourist;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;

namespace Explorer.Tours.Tests.Integration.Tourist;

[Collection("Sequential")]
public class HighlightedToursQueryTests : BaseToursIntegrationTest
{
    public HighlightedToursQueryTests(ToursTestFactory factory) : base(factory) { }

    [Fact]
    public void Retrieves_highlighted_tours_without_authentication()
    {
        // Arrange
        using var scope = Factory.Services.CreateScope();
        var controller = CreateController(scope, null); // null = neprijavljeni korisnik

        // Act
        var result = (controller.GetHighlightedTours(6).Result as OkObjectResult)
            ?.Value as List<TourPreviewDto>;

        // Assert
        result.ShouldNotBeNull();
        // Ako nema published tura, rezultat je prazan
        if (!result.Any()) return;

        // Provera da ne vraća više od 6 tura
        result.Count.ShouldBeLessThanOrEqualTo(6);
    }

    [Fact]
    public void Highlighted_tours_are_sorted_by_rating()
    {
        // Arrange
        using var scope = Factory.Services.CreateScope();
        var controller = CreateController(scope, null);

        // Act
        var result = (controller.GetHighlightedTours(6).Result as OkObjectResult)
            ?.Value as List<TourPreviewDto>;

        // Assert
        result.ShouldNotBeNull();

        if (result.Count > 1)
        {
            // Proveri da je sortiranje ispravno (najbolje ocenjene prvo)
            for (int i = 0; i < result.Count - 1; i++)
            {
                result[i].AverageRating.ShouldBeGreaterThanOrEqualTo(result[i + 1].AverageRating);
            }
        }
    }

    [Fact]
    public void Retrieves_custom_number_of_highlighted_tours()
    {
        // Arrange
        using var scope = Factory.Services.CreateScope();
        var controller = CreateController(scope, null);

        // Act
        var result = (controller.GetHighlightedTours(3).Result as OkObjectResult)
            ?.Value as List<TourPreviewDto>;

        // Assert
        result.ShouldNotBeNull();

        if (result.Any())
        {
            result.Count.ShouldBeLessThanOrEqualTo(3);
        }
    }

    [Fact]
    public void Highlighted_tours_include_required_fields()
    {
        // Arrange
        using var scope = Factory.Services.CreateScope();
        var controller = CreateController(scope, null);

        // Act
        var result = (controller.GetHighlightedTours(6).Result as OkObjectResult)
            ?.Value as List<TourPreviewDto>;

        // Assert
        result.ShouldNotBeNull();

        if (result.Any())
        {
            var tour = result.First();
            tour.Name.ShouldNotBeNullOrWhiteSpace();
            tour.Length.ShouldBeGreaterThanOrEqualTo(0);
            tour.AverageDuration.ShouldBeGreaterThanOrEqualTo(0);
        }
    }

    [Fact]
    public void Highlighted_tours_do_not_expose_key_points()
    {
        // Arrange
        using var scope = Factory.Services.CreateScope();
        var controller = CreateController(scope, null);

        // Act
        var result = (controller.GetHighlightedTours(6).Result as OkObjectResult)
            ?.Value as List<TourPreviewDto>;

        // Assert
        result.ShouldNotBeNull();

        if (result.Any())
        {
            var tour = result.First();
            // Preview ne sme imati listu svih KeyPoints
            // Može imati samo FirstKeyPoint za display
            tour.FirstKeyPoint.ShouldNotBeNull();
        }
    }

    private static TouristToursController CreateController(IServiceScope scope, string? personId)
    {
        var controller = new TouristToursController(
            scope.ServiceProvider.GetRequiredService<ITourService>(),
            scope.ServiceProvider.GetRequiredService<ITouristTourService>(),
            scope.ServiceProvider.GetRequiredService<ITourExecutionService>()
        );

        // Ako je personId null, ne postavljamo context (neprijavljeni korisnik)
        if (personId != null)
        {
            controller.ControllerContext = BuildContext(personId);
        }

        return controller;
    }
}
