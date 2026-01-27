using Explorer.API.Controllers.Tourist.Execution;
using Explorer.Tours.API.Dtos;
using Explorer.Tours.API.Public.Execution;
using Explorer.Tours.Core.Domain;
using Explorer.Tours.Infrastructure.Database;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;

namespace Explorer.Tours.Tests.Integration.Execution;

[Collection("Sequential")]
public class TourHistoryQueryTests : BaseToursIntegrationTest
{
    public TourHistoryQueryTests(ToursTestFactory factory) : base(factory) { }

    [Fact]
    public void Gets_tour_history_with_completed_tours()
    {
        // Arrange
        using var scope = Factory.Services.CreateScope();
        var controller = CreateController(scope, "-21");
        var dbContext = scope.ServiceProvider.GetRequiredService<ToursContext>();

        CleanupTourist(dbContext, -21);
        CreateCompletedToursForTourist(dbContext, -21, 2);

        // Act
        var result = controller.GetTourHistory();

        // Assert
        result.Result.ShouldBeOfType<OkObjectResult>();
        var data = (result.Result as OkObjectResult)?.Value as TourHistoryOverviewDto;

        data.ShouldNotBeNull();
        data.CompletedTours.ShouldNotBeEmpty();
        data.CompletedTours.Count.ShouldBe(2);
    }

    [Fact]
    public void Calculates_correct_statistics()
    {
        // Arrange
        using var scope = Factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ToursContext>();
        var controller = CreateController(scope, "-22");

        CleanupTourist(dbContext, -22);
        CreateCompletedToursForTourist(dbContext, -22, 2); 

        // Act
        var result = controller.GetTourHistory();

        // Assert
        result.Result.ShouldBeOfType<OkObjectResult>();
        var data = ((OkObjectResult)result.Result).Value as TourHistoryOverviewDto;

        data.Statistics.ShouldNotBeNull();
        data.Statistics.TotalCompletedTours.ShouldBe(2);
        data.Statistics.TotalDistanceTraveled.ShouldBe(20.0); // 5.0 + 15.0
        data.Statistics.TotalTimeSpent.ShouldBeGreaterThan(0);
    }

    [Fact]
    public void Provides_comparison_with_average_tourist()
    {
        // Arrange
        using var scope = Factory.Services.CreateScope();
        var controller = CreateController(scope, "-23");
        var dbContext = scope.ServiceProvider.GetRequiredService<ToursContext>();

        CleanupTourist(dbContext, -23);
        CreateCompletedToursForTourist(dbContext, -23, 2);

        // Act
        var result = controller.GetTourHistory();

        // Assert
        var data = ((OkObjectResult)result.Result).Value as TourHistoryOverviewDto;

        data.Comparison.ShouldNotBeNull();
        data.Comparison.YourCompletedTours.ShouldBe(2);
        data.Comparison.AverageCompletedTours.ShouldBeGreaterThanOrEqualTo(0);
    }

    [Fact]
    public void Returns_empty_history_for_tourist_with_no_completed_tours()
    {
        // Arrange
        using var scope = Factory.Services.CreateScope();
        var controller = CreateController(scope, "-999");
        var dbContext = scope.ServiceProvider.GetRequiredService<ToursContext>();

        CleanupTourist(dbContext, -999);

        // Act
        var result = controller.GetTourHistory();

        // Assert
        var data = ((OkObjectResult)result.Result).Value as TourHistoryOverviewDto;

        data.ShouldNotBeNull();
        data.CompletedTours.ShouldBeEmpty();
        data.Statistics.TotalCompletedTours.ShouldBe(0);
        data.Statistics.TotalDistanceTraveled.ShouldBe(0);
    }

    [Fact]
    public void Tour_history_includes_tour_details()
    {
        // Arrange
        using var scope = Factory.Services.CreateScope();
        var controller = CreateController(scope, "-24");
        var dbContext = scope.ServiceProvider.GetRequiredService<ToursContext>();

        CleanupTourist(dbContext, -24);
        CreateCompletedToursForTourist(dbContext, -24, 2);

        // Act
        var result = controller.GetTourHistory();

        // Assert
        var data = ((OkObjectResult)result.Result).Value as TourHistoryOverviewDto;

        var firstTour = data.CompletedTours.First();
        firstTour.TourName.ShouldNotBeNullOrEmpty();
        firstTour.DistanceInKm.ShouldBeGreaterThan(0);
        firstTour.CompletedAt.ShouldNotBe(default(DateTime));
        firstTour.Location.ShouldNotBeNullOrEmpty();
    }

    [Fact]
    public void Statistics_calculates_total_time_correctly()
    {
        // Arrange
        using var scope = Factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ToursContext>();
        var controller = CreateController(scope, "-25");

        CleanupTourist(dbContext, -25);
        CreateCompletedToursForTourist(dbContext, -25, 2);

        // Act
        var result = controller.GetTourHistory();

        // Assert
        var data = ((OkObjectResult)result.Result).Value as TourHistoryOverviewDto;

        data.Statistics.ShouldNotBeNull();
        data.Statistics.TotalTimeSpent.ShouldBeGreaterThan(0);
        // Vreme se računa kao razlika između StartTime i CompletionTime
    }

    // ==================== HELPER METHODS ====================

    private static TourHistoryController CreateController(IServiceScope scope, string touristId)
    {
        return new TourHistoryController(
            scope.ServiceProvider.GetRequiredService<ITourHistoryService>())
        {
            ControllerContext = BuildContext(touristId)
        };
    }

    private static ControllerContext BuildContext(string touristId)
    {
        var user = new ClaimsPrincipal(
            new ClaimsIdentity(
                new[] { new Claim("id", touristId) }, "mock"));
        return new ControllerContext { HttpContext = new DefaultHttpContext { User = user } };
    }

    private static void CleanupTourist(ToursContext dbContext, long touristId)
    {
        var executions = dbContext.TourExecutions
            .Where(te => te.TouristId == touristId)
            .ToList();

        if (executions.Any())
        {
            dbContext.TourExecutions.RemoveRange(executions);
            dbContext.SaveChanges();
        }
    }

    private static void CreateCompletedToursForTourist(ToursContext dbContext, long touristId, int count)
    {
        for (int i = 0; i < count; i++)
        {
            var tour = CreateTour(dbContext, $"Tour {i + 1}", 5.0 + (i * 10.0));

            var execution = new TourExecution(touristId, tour.Id, 45.25 + i, 19.83 + i);
            execution.Complete();
            dbContext.TourExecutions.Add(execution);
        }

        dbContext.SaveChanges();
    }

    private static Tour CreateTour(ToursContext dbContext, string name, double distance)
    {
        var tour = new Tour(name, "Test description", TourDifficulty.Easy, -11);
        tour.UpdateDistance(distance);
        dbContext.Tours.Add(tour);
        dbContext.SaveChanges();

        // Dodaj bar jedan KeyPoint (potreban za Location field)
        dbContext.KeyPoints.Add(new KeyPoint(
            tour.Id, 
            "Start Point", 
            "Description", 
            "img.jpg", 
            "secret", 
            45.25, 
            19.83));
        dbContext.SaveChanges();

        return tour;
    }
}
