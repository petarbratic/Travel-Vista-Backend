using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Explorer.BuildingBlocks.Tests;
using Explorer.Stakeholders.Infrastructure.Database;
using Explorer.Tours.Infrastructure.Database;
using Explorer.Payments.Infrastructure.Database;

namespace Explorer.Stakeholders.Tests;

public class StakeholdersTestFactory : BaseTestFactory<StakeholdersContext>
{
    protected override IServiceCollection ReplaceNeededDbContexts(IServiceCollection services)
    {
        var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<StakeholdersContext>));
        services.Remove(descriptor!);
        services.AddDbContext<StakeholdersContext>(SetupTestContext());

        var toursDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<ToursContext>));
        services.Remove(toursDescriptor!);
        services.AddDbContext<ToursContext>(SetupTestContext());

        var paymentsDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<PaymentsContext>));
        services.Remove(paymentsDescriptor!);
        services.AddDbContext<PaymentsContext>(SetupTestContext());

        var blogDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<Explorer.Blog.Infrastructure.Database.BlogContext>));
        services.Remove(blogDescriptor!);
        services.AddDbContext<Explorer.Blog.Infrastructure.Database.BlogContext>(SetupTestContext());

        var encounterDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<Explorer.Encounters.Infrastructure.Database.EncountersContext>));
        services.Remove(encounterDescriptor!);
        services.AddDbContext<Explorer.Encounters.Infrastructure.Database.EncountersContext>(SetupTestContext());

        return services;
    }
}