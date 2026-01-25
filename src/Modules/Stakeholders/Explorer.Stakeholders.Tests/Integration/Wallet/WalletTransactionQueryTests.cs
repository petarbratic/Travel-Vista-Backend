using Explorer.API.Controllers.Tourist;
using Explorer.Stakeholders.API.Dtos;
using Explorer.Stakeholders.API.Public;
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
    public class WalletTransactionQueryTests : BaseStakeholdersIntegrationTest
    {
        public WalletTransactionQueryTests(StakeholdersTestFactory factory) : base(factory) { }

        private static ClaimsPrincipal BuildUser(long personId)
        {
            var claims = new[] { new Claim("personId", personId.ToString()) };
            return new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuth"));
        }

        private static ControllerContext BuildContext(long personId)
            => new ControllerContext { HttpContext = new DefaultHttpContext { User = BuildUser(personId) } };

        private static TouristWalletController CreateController(IServiceScope scope, long personId)
        {
            var service = scope.ServiceProvider.GetRequiredService<IWalletService>();
            return new TouristWalletController(service) { ControllerContext = BuildContext(personId) };
        }

        [Fact]
        public void Tourist_can_get_my_transactions_paged()
        {
            using var scope = Factory.Services.CreateScope();
            var controller = CreateController(scope, -21);

            // Act
            var actionResult = controller.GetMyTransactions(page: 1, pageSize: 2);
            var ok = actionResult.Result as OkObjectResult;

            // Assert - response
            ok.ShouldNotBeNull();
            var dto = ok!.Value.ShouldBeOfType<PagedResultDto<WalletTransactionDto>>();

            dto.Page.ShouldBe(1);
            dto.PageSize.ShouldBe(2);
            dto.TotalCount.ShouldBeGreaterThan(0);
            dto.Items.Count.ShouldBeLessThanOrEqualTo(2);

            dto.Items[0].Id.ShouldNotBe(0);
            dto.Items[0].CreatedAtUtc.ShouldNotBe(default);
            dto.Items[0].Description.ShouldNotBeNullOrWhiteSpace();
        }

        [Fact]
        public void Author_cannot_get_my_transactions()
        {
            using var scope = Factory.Services.CreateScope();
            var controller = CreateController(scope, -11); // autor

            Should.Throw<UnauthorizedAccessException>(() => controller.GetMyTransactions());
        }

        [Fact]
        public void Admin_cannot_get_my_transactions()
        {
            using var scope = Factory.Services.CreateScope();
            var controller = CreateController(scope, -1); // admin

            Should.Throw<UnauthorizedAccessException>(() => controller.GetMyTransactions());
        }
    }
}
