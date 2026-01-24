// src/Modules/Tours/Explorer.Tours.Tests/Integration/Tourist/TourAccessTests.cs
using System;
using System.Linq;
using Explorer.API.Controllers.Shopping;
using Explorer.API.Controllers.Tourist;
using Explorer.API.Controllers.Tourist.Execution;
using Explorer.Payments.API.Dtos;
using Explorer.Payments.API.Public.Shopping;
using Explorer.Payments.Infrastructure.Database;
using Explorer.Tours.API.Dtos;
using Explorer.Tours.API.Public.Execution;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;

namespace Explorer.Tours.Tests.Integration.Tourist
{
    [Collection("Sequential")]
    public class TourAccessTests : BaseToursIntegrationTest
    {
        public TourAccessTests(ToursTestFactory factory) : base(factory) { }

        // Koristimo različite test turiste za različite testove
        private const string TestTourist1 = "-21"; // Bronze rank, 5000 AC
        private const string TestTourist2 = "-22"; // Silver rank, 5000 AC
        private const string TestTourist3 = "-23"; // Gold rank, 5000 AC

        // Helper za čišćenje korpe
        private void ClearCart(IServiceScope scope, long touristId)
        {
            var db = scope.ServiceProvider.GetRequiredService<PaymentsContext>();
            var cart = db.ShoppingCarts.FirstOrDefault(c => c.TouristId == touristId);
            if (cart != null)
            {
                cart.Clear();
                db.SaveChanges();
            }
        }

        [Fact]
        public void Details_returns_no_keypoints_for_unpurchased_tour()
        {
            long tourId = -2;
            var testTourist = TestTourist1;
            var touristId = long.Parse(testTourist);

            using var scope = Factory.Services.CreateScope();

            // Očisti korpu da sigurno tura nije kupljena
            ClearCart(scope, touristId);

            var controller = CreateTouristToursController(scope, testTourist);

            var result = controller.GetTourDetails(tourId).Result as OkObjectResult;
            result.ShouldNotBeNull();

            var dto = result.Value as TourDetailsDto;
            dto.ShouldNotBeNull();
            dto.KeyPoints.ShouldBeNull();
        }

        [Fact]
        public void Details_returns_keypoints_after_purchase()
        {
            long tourId = -2;
            var testTourist = TestTourist2; // Koristi drugog turista
            var touristId = long.Parse(testTourist);

            using var scope = Factory.Services.CreateScope();

            // Očisti korpu pre kupovine
            ClearCart(scope, touristId);

            var cart = CreateCartController(scope, testTourist);
            var purchase = CreatePurchaseController(scope, testTourist);
            var controller = CreateTouristToursController(scope, testTourist);

            cart.Add(new ShoppingCartRequestDto { TourId = tourId });

            var checkoutActionResult = purchase.Checkout();
            var checkoutOk = checkoutActionResult.Result as OkObjectResult;
            checkoutOk.ShouldNotBeNull();
            var checkoutResult = checkoutOk.Value as CheckoutResultDto;
            checkoutResult.ShouldNotBeNull();
            checkoutResult.Success.ShouldBeTrue();

            var result = controller.GetTourDetails(tourId).Result as OkObjectResult;
            result.ShouldNotBeNull();

            var dto = result.Value as TourDetailsDto;
            dto.ShouldNotBeNull();
            dto.KeyPoints.ShouldNotBeNull();
            dto.KeyPoints.Count.ShouldBeGreaterThan(0);

            typeof(KeyPointPublicDto)
                .GetProperties()
                .Any(p => p.Name == "Secret")
                .ShouldBeFalse();
        }

        [Fact]
        public void Cannot_purchase_archived_tour()
        {
            long tourId = -3; // Arhivirana tura
            var testTourist = TestTourist1;
            var touristId = long.Parse(testTourist);

            using var scope = Factory.Services.CreateScope();

            // Očisti korpu
            ClearCart(scope, touristId);

            var cart = CreateCartController(scope, testTourist);

            Should.Throw<InvalidOperationException>(() =>
            {
                cart.Add(new ShoppingCartRequestDto { TourId = tourId });
            });
        }

        [Fact]
        public void Cannot_start_unpurchased_tour()
        {
            long tourId = -4; // Koristi drugu turu koja sigurno nije kupljena
            var testTourist = TestTourist1;
            var touristId = long.Parse(testTourist);

            using var scope = Factory.Services.CreateScope();

            // Očisti korpu da sigurno tura nije kupljena
            ClearCart(scope, touristId);

            var exec = CreateExecutionController(scope, testTourist);

            exec.ControllerContext.HttpContext.User.Identities.First()
                .AddClaim(new System.Security.Claims.Claim("id", testTourist));

            var request = new TourExecutionCreateDto
            {
                TourId = tourId,
                StartLatitude = 45.25,
                StartLongitude = 19.83
            };

            var result = exec.StartTour(request).Result;

            // Očekujemo BadRequest jer tura nije kupljena
            result.ShouldBeOfType<BadRequestObjectResult>();

            var badRequest = result as BadRequestObjectResult;
            badRequest.ShouldNotBeNull();
            // Opciono: proveri poruku greške
            // badRequest.Value.ShouldContain("not purchased");
        }

        [Fact]
        public void Can_start_purchased_tour()
        {
            long tourId = -2;
            var testTourist = TestTourist3; // Koristi trećeg turista
            var touristId = long.Parse(testTourist);

            using var scope = Factory.Services.CreateScope();

            // Očisti korpu pre kupovine
            ClearCart(scope, touristId);

            var cart = CreateCartController(scope, testTourist);
            var purchase = CreatePurchaseController(scope, testTourist);
            var exec = CreateExecutionController(scope, testTourist);

            exec.ControllerContext.HttpContext.User.Identities.First()
                .AddClaim(new System.Security.Claims.Claim("id", testTourist));

            cart.Add(new ShoppingCartRequestDto { TourId = tourId });

            var checkoutActionResult = purchase.Checkout();
            var checkoutOk = checkoutActionResult.Result as OkObjectResult;
            checkoutOk.ShouldNotBeNull();
            var checkoutResult = checkoutOk.Value as CheckoutResultDto;
            checkoutResult.ShouldNotBeNull();
            checkoutResult.Success.ShouldBeTrue();

            var request = new TourExecutionCreateDto
            {
                TourId = tourId,
                StartLatitude = 45.25,
                StartLongitude = 19.83
            };

            var result = exec.StartTour(request).Result;
            result.ShouldBeOfType<OkObjectResult>();
        }

        // Helperi
        private TouristToursController CreateTouristToursController(IServiceScope scope, string personId)
        {
            return new TouristToursController(
                scope.ServiceProvider.GetRequiredService<Explorer.Tours.API.Public.Authoring.ITourService>(),
                scope.ServiceProvider.GetRequiredService<Explorer.Tours.API.Public.Tourist.ITouristTourService>(),
                scope.ServiceProvider.GetRequiredService<ITourExecutionService>()
            )
            {
                ControllerContext = BuildContext(personId)
            };
        }

        private ShoppingCartController CreateCartController(IServiceScope scope, string personId)
        {
            return new ShoppingCartController(
                scope.ServiceProvider.GetRequiredService<IShoppingCartService>())
            {
                ControllerContext = BuildContext(personId)
            };
        }

        private TourPurchaseController CreatePurchaseController(IServiceScope scope, string personId)
        {
            return new TourPurchaseController(
                scope.ServiceProvider.GetRequiredService<ITourPurchaseTokenService>())
            {
                ControllerContext = BuildContext(personId)
            };
        }

        private TourExecutionController CreateExecutionController(IServiceScope scope, string personId)
        {
            return new TourExecutionController(
                scope.ServiceProvider.GetRequiredService<ITourExecutionService>())
            {
                ControllerContext = BuildContext(personId)
            };
        }
    }
}