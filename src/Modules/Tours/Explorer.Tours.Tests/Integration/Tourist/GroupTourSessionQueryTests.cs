using Explorer.API.Controllers.Tourist;
using Explorer.API.Controllers.Tourist.Execution;
using Explorer.Payments.API.Public.Shopping;
using Explorer.Stakeholders.API.Public;
using Explorer.Tours.API.Dtos;
using Explorer.Tours.API.Public.Authoring;
using Explorer.Tours.API.Public.Execution;
using Explorer.Tours.API.Public.Tourist;
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
using System.Text;
using System.Threading.Tasks;

namespace Explorer.Tours.Tests.Integration.Tourist
{
    [Collection("Sequential")]
    public class GroupTourSessionQueryTests : BaseToursIntegrationTest
    {
        public GroupTourSessionQueryTests(ToursTestFactory factory) : base(factory) { }

        [Fact]
        public void Retrieves_group_tour_session_by_id()
        {
            using var scope = Factory.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ToursContext>();
            var shoppingCart = scope.ServiceProvider.GetRequiredService<IShoppingCartService>();
            var tokenService = scope.ServiceProvider.GetRequiredService<ITourPurchaseTokenService>();

            var controller = CreateSessionController(scope, "-23");

            var teController = CreateTourExecutionController(scope, "-23");

            teController.AbandonTour();

            shoppingCart.AddToCart(-23, -2);
            tokenService.Checkout(-23);

            var createSessionDto = new CreateGroupTourSessionDto
            {
                ClubId = -2,
                TourId = -2,
                TourName = "Test Tour Published",
            };

            var createResult = controller.CreateGroupTourSession(createSessionDto);

            var createdSession = (createResult.Result as CreatedAtActionResult)!.Value as GroupTourSessionDto;

            var retrieveResult = controller.GetSessionById(createdSession!.Id);

            var retrievedSession = (retrieveResult.Result as OkObjectResult)!.Value as GroupTourSessionDto;

            retrievedSession!.Id.ShouldBe(createdSession.Id);
        }

        [Fact]
        public void Retrieve_active_sessions_by_club_id()
        {
            using var scope = Factory.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ToursContext>();
            var shoppingCart = scope.ServiceProvider.GetRequiredService<IShoppingCartService>();
            var tokenService = scope.ServiceProvider.GetRequiredService<ITourPurchaseTokenService>();
            var admin = scope.ServiceProvider.GetRequiredService<IWalletService>();

            var controller = CreateSessionController(scope, "-23");

            var teController = CreateTourExecutionController(scope, "-23");

            teController.AbandonTour();

            shoppingCart.AddToCart(-23, -2);
            tokenService.Checkout(-23);

            var createSessionDto = new CreateGroupTourSessionDto
            {
                ClubId = -2,
                TourId = -2,
                TourName = "Test Tour Published",
            };

            var result1 = controller.CreateGroupTourSession(createSessionDto);
            var createdSession = (result1.Result as CreatedAtActionResult)!.Value as GroupTourSessionDto;
            controller = CreateSessionController(scope, "-22");

            teController = CreateTourExecutionController(scope, "-22");

            teController.AbandonTour();

            admin.TopUp(-1, -22, 1000);

            shoppingCart.AddToCart(-22, -2);
            tokenService.Checkout(-22);

            controller.JoinGroupTourSession(createdSession!.Id, -22);

            var result = controller.GetActiveSessionsByClubId(-2);
            var sessions = (result.Result as OkObjectResult)!.Value as IEnumerable<GroupTourSessionDto>;
            sessions!.Count().ShouldBe(1);

            controller = CreateSessionController(scope, "-23");

            teController = CreateTourExecutionController(scope, "-23");

            teController.AbandonTour();

            createSessionDto = new CreateGroupTourSessionDto
            {
                ClubId = -2,
                TourId = -4,
                TourName = "Test Tour Published 2",
            };
            controller.CreateGroupTourSession(createSessionDto);
            result = controller.GetActiveSessionsByClubId(-2);
            var sessions2 = (result.Result as OkObjectResult)!.Value as IEnumerable<GroupTourSessionDto>;
            sessions2!.Count().ShouldBe(2);
        }

        [Fact]
        public void Retrieve_ended_sessions_by_club_id()
        {
            using var scope = Factory.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ToursContext>();
            var shoppingCart = scope.ServiceProvider.GetRequiredService<IShoppingCartService>();
            var tokenService = scope.ServiceProvider.GetRequiredService<ITourPurchaseTokenService>();

            var controller = CreateSessionController(scope, "-23");

            var teController = CreateTourExecutionController(scope, "-23");

            teController.AbandonTour();

            shoppingCart.AddToCart(-23, -2);
            tokenService.Checkout(-23);

            var createSessionDto = new CreateGroupTourSessionDto
            {
                ClubId = -2,
                TourId = -2,
                TourName = "Test Tour Published",
            };

            controller.CreateGroupTourSession(createSessionDto);

            controller = CreateSessionController(scope, "-22");

            teController = CreateTourExecutionController(scope, "-22");

            teController.AbandonTour();

            var tourId = CreateAndPublishTour(scope);

            shoppingCart.AddToCart(-22, tourId);
            tokenService.Checkout(-22);

            createSessionDto = new CreateGroupTourSessionDto
            {
                ClubId = -2,
                TourId = tourId,
                TourName = "Created tour test"
            };

            controller.CreateGroupTourSession(createSessionDto);

            teController.AbandonTour();

            var result = controller.GetSessionsForHighlightMarking(-2);
            var sessions = (result.Result as OkObjectResult)!.Value as IEnumerable<GroupTourSessionDto>;
            sessions!.Count().ShouldBe(1);
        }

        private static GroupTourSessionController CreateSessionController(IServiceScope scope, string touristId)
        {
            return new GroupTourSessionController(scope.ServiceProvider.GetRequiredService<IGroupTourSessionService>())
            {
                ControllerContext = BuildContext(touristId)
            };
        }

        private static TourExecutionController CreateTourExecutionController(IServiceScope scope, string touristId)
        {
            return new TourExecutionController(scope.ServiceProvider.GetRequiredService<ITourExecutionService>())
            {
                ControllerContext = BuildIdContext(touristId)
            };
        }

        private static ControllerContext BuildIdContext(string touristId)
        {
            var user = new ClaimsPrincipal(
                new ClaimsIdentity(new[] { new Claim("id", touristId) }, authenticationType: "mock"));

            return new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };
        }

        private static long CreateAndPublishTour(IServiceScope scope)
        {
            var tourService = scope.ServiceProvider.GetRequiredService<ITourService>();
            var db = scope.ServiceProvider.GetRequiredService<ToursContext>();

            var tour = tourService.Create(new TourCreateDto
            {
                Name = "Location Test Tour",
                Description = "Desc",
                Difficulty = 0,
                Tags = new List<string> { "loc" }
            }, -11);

            tourService.Update(new TourUpdateDto
            {
                Id = tour.Id,
                Name = tour.Name,
                Description = tour.Description,
                Difficulty = tour.Difficulty,
                Price = 100,
                Tags = new List<string> { "loc" },
                TourDurations = new List<TourDurationDto>
            {
                new TourDurationDto { TimeInMinutes = 60, TransportType = 0 }
            }
            }, -11);

            db.KeyPoints.Add(new KeyPoint(tour.Id, "KP1", "D1", "u", "s", 45, 19));
            db.KeyPoints.Add(new KeyPoint(tour.Id, "KP2", "D2", "u", "s", 46, 20));
            db.SaveChanges();

            tourService.Publish(tour.Id, -11);
            return tour.Id;
        }
    }
}
