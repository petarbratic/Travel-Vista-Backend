using System;
using System.Linq;
using Explorer.API.Controllers.Shopping;
using Explorer.Payments.API.Dtos;
using Explorer.Payments.API.Public.Shopping;
using Explorer.Payments.Infrastructure.Database;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Xunit;

namespace Explorer.Payments.Tests.Integration.Shopping
{
    [Collection("Sequential")]
    public class TourPurchaseTokenIntegrationTests : BasePaymentsIntegrationTest
    {
        public TourPurchaseTokenIntegrationTests(PaymentsTestFactory factory)
            : base(factory) { }

        private const string TestTourist1 = "-21";
        private const string TestTourist2 = "-22";

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

        // Helper za čišćenje tokena
        private void ClearTokens(IServiceScope scope, long touristId)
        {
            var db = scope.ServiceProvider.GetRequiredService<PaymentsContext>();
            var tokens = db.TourPurchaseTokens.Where(t => t.TouristId == touristId).ToList();
            if (tokens.Any())
            {
                db.TourPurchaseTokens.RemoveRange(tokens);
                db.SaveChanges();
            }
        }

        [Fact]
        public void Checkout_creates_tokens_and_empties_cart()
        {
            var personId = TestTourist1;
            var touristId = long.Parse(personId);
            var tourId = -2;

            using var scope = Factory.Services.CreateScope();

            // Očisti korpu i tokene pre testa
            ClearCart(scope, touristId);
            ClearTokens(scope, touristId);

            var cartController = new ShoppingCartController(
                scope.ServiceProvider.GetRequiredService<IShoppingCartService>())
            {
                ControllerContext = BuildContext(personId)
            };

            var purchaseController = new TourPurchaseController(
                scope.ServiceProvider.GetRequiredService<ITourPurchaseTokenService>())
            {
                ControllerContext = BuildContext(personId)
            };

            var db = scope.ServiceProvider.GetRequiredService<PaymentsContext>();

            cartController.Add(new ShoppingCartRequestDto { TourId = tourId });

            var actionResult = purchaseController.Checkout();
            actionResult.ShouldNotBeNull();

            var okResult = actionResult.Result as OkObjectResult;
            okResult.ShouldNotBeNull();

            var result = okResult.Value as CheckoutResultDto;
            result.ShouldNotBeNull();
            result.Success.ShouldBeTrue();
            result.Tokens.Count.ShouldBe(1);
            result.Tokens[0].TourId.ShouldBe(tourId);
            result.Tokens[0].TouristId.ShouldBe(touristId);

            // Proveri da token postoji u bazi sa istim tourId i touristId
            var stored = db.TourPurchaseTokens
                            .FirstOrDefault(t => t.TouristId == touristId && t.TourId == tourId);
            stored.ShouldNotBeNull();
            
            // Token GUID se generiše random, pa samo proveravamo da postoji i da se poklapa
            stored.Token.ShouldBe(result.Tokens[0].Token);
            stored.Token.ShouldNotBeNullOrEmpty();

            var storedCart = db.ShoppingCarts.First(c => c.TouristId == touristId);
            storedCart.Items.Count.ShouldBe(0);
            storedCart.TotalPrice.ShouldBe(0);
        }

        [Fact]
        public void GetTokens_returns_all_tokens_for_tourist()
        {
            var personId = TestTourist2;
            var touristId = long.Parse(personId);
            var tour1 = -2;
            var tour2 = -4;

            using var scope = Factory.Services.CreateScope();

            // Očisti korpu i tokene pre testa
            ClearCart(scope, touristId);
            ClearTokens(scope, touristId);

            var cart = new ShoppingCartController(
                scope.ServiceProvider.GetRequiredService<IShoppingCartService>())
            { ControllerContext = BuildContext(personId) };

            var purchase = new TourPurchaseController(
                scope.ServiceProvider.GetRequiredService<ITourPurchaseTokenService>())
            { ControllerContext = BuildContext(personId) };

            cart.Add(new ShoppingCartRequestDto { TourId = tour1 });
            cart.Add(new ShoppingCartRequestDto { TourId = tour2 });

            var checkoutActionResult = purchase.Checkout();
            var checkoutOk = checkoutActionResult.Result as OkObjectResult;
            checkoutOk.ShouldNotBeNull();
            var checkoutResult = checkoutOk.Value as CheckoutResultDto;
            checkoutResult.ShouldNotBeNull();
            checkoutResult.Success.ShouldBeTrue();

            var tokensActionResult = purchase.GetTokens();
            var tokensOk = tokensActionResult.Result as OkObjectResult;
            tokensOk.ShouldNotBeNull();
            var result = tokensOk.Value as System.Collections.Generic.List<TourPurchaseTokenDto>;

            result.ShouldNotBeNull();
            result.Count.ShouldBe(2);
            result.Select(t => t.TourId).ShouldContain(tour1);
            result.Select(t => t.TourId).ShouldContain(tour2);
        }
    }
}