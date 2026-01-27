using Explorer.API.Controllers.Administrator.Administration;
using Explorer.Stakeholders.API.Dtos;
using Explorer.Stakeholders.API.Internal;
using Explorer.Stakeholders.API.Public;
using Explorer.Stakeholders.Core.Domain;
using Explorer.Stakeholders.Infrastructure.Database;
using Explorer.Tours.API.Public;
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


namespace Explorer.Stakeholders.Tests.Integration.Wallet
{
    [Collection("Sequential")]
    public class WalletTransactionCommandTests : BaseStakeholdersIntegrationTest
    {
        public WalletTransactionCommandTests(StakeholdersTestFactory factory) : base(factory) { }

        private static ClaimsPrincipal BuildUser(long personId)
        {
            var claims = new[] { new Claim("personId", personId.ToString()) };
            return new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuth"));
        }

        private static ControllerContext BuildContext(long personId)
            => new ControllerContext { HttpContext = new DefaultHttpContext { User = BuildUser(personId) } };

        private static AdminWalletController CreateAdminController(IServiceScope scope, long adminPersonId)
        {
            var walletService = scope.ServiceProvider.GetRequiredService<IWalletService>();
            var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();

            return new AdminWalletController(walletService, notificationService)
            {
                ControllerContext = BuildContext(adminPersonId)
            };
        }

        [Fact]
        public void Topup_creates_wallet_transaction_in_database()
        {
            // Arrange
            using var scope = Factory.Services.CreateScope();
            var controller = CreateAdminController(scope, -1);

            var dbContext = scope.ServiceProvider.GetRequiredService<StakeholdersContext>();

            // Act
            var request = new WalletTopUpDto { TouristUserId = -21, AmountAc = 25 };
            controller.TopUp(request);

            // Assert - Database
            var tx = dbContext.WalletTransactions
                .Where(t => t.PersonId == -21)
                .OrderByDescending(t => t.CreatedAtUtc)
                .FirstOrDefault();

            tx.ShouldNotBeNull();
            tx!.Type.ShouldBe(WalletTransactionType.AdminTopUp);
            tx.AmountAc.ShouldBe(25);
            tx.Description.ShouldContain("Admin top-up");
            tx.InitiatorPersonId.ShouldBe(-1);
        }

        [Fact]
        public void Debit_creates_negative_transaction()
        {
            using var scope = Factory.Services.CreateScope();

            var internalWallet = scope.ServiceProvider.GetRequiredService<IInternalWalletService>();
            var dbContext = scope.ServiceProvider.GetRequiredService<StakeholdersContext>();

            // Act
            internalWallet.Debit(
                personId: -21,
                amountAc: 90,
                type: (int)WalletTransactionType.CheckoutPurchase,
                description: "Checkout purchase (-90 AC)",
                referenceType: "Checkout",
                referenceId: 999
            );

            // Assert - DB
            var tx = dbContext.WalletTransactions
                .Where(t => t.PersonId == -21)
                .OrderByDescending(t => t.CreatedAtUtc)
                .FirstOrDefault();

            tx.ShouldNotBeNull();
            tx!.Type.ShouldBe(WalletTransactionType.CheckoutPurchase);
            tx.AmountAc.ShouldBe(-90);
            tx.ReferenceType.ShouldBe("Checkout");
            tx.ReferenceId.ShouldBe(999);
        }
    }
}
