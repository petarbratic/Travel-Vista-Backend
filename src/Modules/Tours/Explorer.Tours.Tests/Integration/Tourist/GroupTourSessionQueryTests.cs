using Explorer.API.Controllers.Tourist;
using Explorer.API.Controllers.Tourist.Execution;
using Explorer.Tours.API.Dtos;
using Explorer.Tours.API.Public.Execution;
using Explorer.Tours.API.Public.Tourist;
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

            var controller = CreateSessionController(scope, "-21");

            var teController = CreateTourExecutionController(scope, "-21");

            teController.AbandonTour();

            var createSessionDto = new CreateGroupTourSessionDto
            {
                ClubId = -2,
                TourId = -2,
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

            var controller = CreateSessionController(scope, "-21");

            var teController = CreateTourExecutionController(scope, "-21");

            teController.AbandonTour();

            var createSessionDto = new CreateGroupTourSessionDto
            {
                ClubId = -2,
                TourId = -2,
            };

            controller.CreateGroupTourSession(createSessionDto);

            controller = CreateSessionController(scope, "-22");

            teController = CreateTourExecutionController(scope, "-22");

            teController.AbandonTour();

            createSessionDto = new CreateGroupTourSessionDto
            {
                ClubId = -2,
                TourId = -2,
            };

            controller.CreateGroupTourSession(createSessionDto);

            var result = controller.GetActiveSessionsByClubId(-2);
            var sessions = (result.Result as OkObjectResult)!.Value as IEnumerable<GroupTourSessionDto>;
            sessions!.Count().ShouldBe(1);

            controller = CreateSessionController(scope, "-23");

            teController = CreateTourExecutionController(scope, "-23");

            teController.AbandonTour();

            createSessionDto = new CreateGroupTourSessionDto
            {
                ClubId = -2,
                TourId = -3,
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

            var controller = CreateSessionController(scope, "-21");

            var teController = CreateTourExecutionController(scope, "-21");

            teController.AbandonTour();

            var createSessionDto = new CreateGroupTourSessionDto
            {
                ClubId = -2,
                TourId = -2,
            };

            controller.CreateGroupTourSession(createSessionDto);

            controller = CreateSessionController(scope, "-22");

            teController = CreateTourExecutionController(scope, "-22");

            teController.AbandonTour();

            createSessionDto = new CreateGroupTourSessionDto
            {
                ClubId = -2,
                TourId = -100,
            };

            controller.CreateGroupTourSession(createSessionDto);

            teController.AbandonTour();

            var result = controller.GetEndedSessionsByClubId(-2);
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
    }
}
