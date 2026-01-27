using Explorer.API.Controllers.Tourist;
using Explorer.API.Controllers.Tourist.Execution;
using Explorer.Payments.API.Public.Shopping;
using Explorer.Tours.API.Dtos;
using Explorer.Tours.API.Public.Execution;
using Explorer.Tours.API.Public.Tourist;
using Explorer.Tours.Infrastructure.Database;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
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
    public class GroupTourSessionCommandTests : BaseToursIntegrationTest
    {
        public GroupTourSessionCommandTests(ToursTestFactory factory) : base(factory) { }

        [Fact]
        public void Creates_group_tour_session()
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

            var sessionInDb = dbContext.GroupTourSessions
                .OrderByDescending(s => s.Id)
                .FirstOrDefault();

            sessionInDb.ShouldNotBeNull();
            sessionInDb!.ClubId.ShouldBe(-2);

            var participantInDb = dbContext.GroupTourSessionParticipants
                .Where(p=> p.SessionId == sessionInDb.Id)
                .FirstOrDefault();

            participantInDb.ShouldNotBeNull();
            participantInDb!.TouristId.ShouldBe(-23);

            var tourExecutionInDb = dbContext.TourExecutions
                .Where(te => te.Id == participantInDb.TourExecutionId && te.GroupSessionId == sessionInDb.Id)
                .FirstOrDefault();

            createResult.Result.ShouldBeOfType<CreatedAtActionResult>();

            var createdSession = (createResult.Result as CreatedAtActionResult)!.Value as GroupTourSessionDto;
            createdSession.ShouldNotBeNull();
            createdSession.ClubId.ShouldBe(-2);
            createdSession.Participants[0].TouristId.ShouldBe(-23);
        }

        [Fact]
        public void Cannot_create_session()
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

            var sessionInDb = dbContext.GroupTourSessions
                .OrderByDescending(s => s.Id)
                .FirstOrDefault();

            sessionInDb.ShouldNotBeNull();
            sessionInDb!.ClubId.ShouldBe(-2);

            var participantInDb = dbContext.GroupTourSessionParticipants
                .Where(p => p.SessionId == sessionInDb.Id)
                .FirstOrDefault();

            participantInDb.ShouldNotBeNull();
            participantInDb!.TouristId.ShouldBe(-23);

            var tourExecutionInDb = dbContext.TourExecutions
                .Where(te => te.Id == participantInDb.TourExecutionId && te.GroupSessionId == sessionInDb.Id)
                .FirstOrDefault();

            createResult = controller.CreateGroupTourSession(createSessionDto);
            createResult.Result.ShouldBeOfType<BadRequestObjectResult>();          
        }

        [Fact]
        public void Leave_group_session()
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
            var leaveResult = controller.LeaveGroupTourSession(createdSession!.Id, createdSession.Participants[0].TouristId);

            leaveResult.Result.ShouldBeOfType<OkObjectResult>();
            var leftSession = (leaveResult.Result as OkObjectResult)!.Value as GroupTourSessionDto;
            leftSession.ShouldNotBeNull();

            var participantInDb = dbContext.GroupTourSessionParticipants
                .Where(p => p.SessionId == createdSession.Id && p.TouristId == -23)
                .FirstOrDefault();            
            participantInDb!.LeftAt.ShouldNotBeNull();

            var groupSessionInDb = dbContext.GroupTourSessions
                .Include(s => s.Participants)
                .Where(s => s.Id == participantInDb.SessionId)
                .FirstOrDefault();
            groupSessionInDb!.Status.ShouldBe(Core.Domain.GroupTourSessionStatus.Ended);
            groupSessionInDb!.Participants.Where(p => p.LeftAt == null).ToList().Count.ShouldBe(0);
        }

        [Fact]
        public void Join_group_session()
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

            controller = CreateSessionController(scope, "-22");
            teController = CreateTourExecutionController(scope, "-22");
            teController.AbandonTour();
            shoppingCart.AddToCart(-22, -2);
            tokenService.Checkout(-22);

            var participant = new GroupTourSessionParticipantDto
            {
                SessionId = createdSession!.Id,
                TouristId = -22
            };

            var joinResult = controller.JoinGroupTourSession(participant.SessionId, participant.TouristId);

            joinResult.Result.ShouldBeOfType<OkObjectResult>();
            var joinedSession = (joinResult.Result as OkObjectResult)!.Value as GroupTourSessionDto;
            joinedSession.ShouldNotBeNull();

            var participantInDb = dbContext.GroupTourSessionParticipants
                .Where(p => p.SessionId == createdSession.Id && p.TouristId == -22)
                .FirstOrDefault();
            participantInDb!.LeftAt.ShouldBeNull();

            var groupSessionInDb = dbContext.GroupTourSessions
                .Include(s => s.Participants)
                .Where(s => s.Id == participantInDb.SessionId)
                .FirstOrDefault();
            groupSessionInDb!.Status.ShouldBe(Core.Domain.GroupTourSessionStatus.Active);
            groupSessionInDb!.Participants.Where(p => p.LeftAt == null).ToList().Count.ShouldBe(2);
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
    }
}
