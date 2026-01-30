using Explorer.API.Controllers.Shopping;
using Explorer.Payments.API.Dtos;
using Explorer.Payments.API.Public.Shopping;
using Explorer.Payments.Infrastructure.Database;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using System.Linq;

namespace Explorer.Payments.Tests.Integration.Shopping;

[Collection("Sequential")]
public class TourPurchaseTokenQueryTests : BasePaymentsIntegrationTest
{
    public TourPurchaseTokenQueryTests(PaymentsTestFactory factory) : base(factory) { }

    private const string TestTourist = "-23";

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
    public void GetTokens_returns_all_for_tourist()
    {
        var personId = TestTourist;
        var personIdLong = long.Parse(personId);
        var tour1 = -2;
        var tour2 = -4;

        using var scope = Factory.Services.CreateScope();

        // Očisti korpu i tokene pre testa
        ClearCart(scope, personIdLong);
        ClearTokens(scope, personIdLong);

        var cart = CreateCartController(scope, personId);
        var purchase = CreatePurchaseController(scope, personId);

        cart.Add(new ShoppingCartRequestDto { TourId = tour1 });
        cart.Add(new ShoppingCartRequestDto { TourId = tour2 });

        purchase.Checkout();

        var result = ((ObjectResult)purchase.GetTokens().Result)?.Value
                     as List<TourPurchaseTokenDto>;

        result.ShouldNotBeNull();
        result.Count.ShouldBe(2);
        result.Select(t => t.TourId).ShouldContain(tour1);
        result.Select(t => t.TourId).ShouldContain(tour2);
    }

    private static ShoppingCartController CreateCartController(IServiceScope scope, string personId)
    {
        return new ShoppingCartController(scope.ServiceProvider.GetRequiredService<IShoppingCartService>())
        {
            ControllerContext = BuildContext(personId)
        };
    }

    private static TourPurchaseController CreatePurchaseController(IServiceScope scope, string personId)
    {
        return new TourPurchaseController(scope.ServiceProvider.GetRequiredService<ITourPurchaseTokenService>())
        {
            ControllerContext = BuildContext(personId)
        };
    }
}