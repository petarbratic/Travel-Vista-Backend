using Explorer.Blog.Infrastructure.Database;
using Explorer.BuildingBlocks.Tests;
using Explorer.Encounters.Infrastructure.Database;
using Explorer.Payments.Infrastructure.Database;
using Explorer.Stakeholders.API.Public;
using Explorer.Stakeholders.Core.UseCases;
using Explorer.Tours.API.Public;
using Explorer.Tours.Core.UseCases;
using Explorer.Stakeholders.Infrastructure.Database;
using Explorer.Tours.Infrastructure.Database;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Explorer.Blog.Tests;

public class BlogTestFactory : BaseTestFactory<BlogContext>
{
    protected override IServiceCollection ReplaceNeededDbContexts(IServiceCollection services)
    {
        var database = Environment.GetEnvironmentVariable("DATABASE_SCHEMA") ?? "explorer-v1-test";

        var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<BlogContext>));
        services.Remove(descriptor!);
        services.AddDbContext<BlogContext>(SetupTestContext());

        var toursDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<ToursContext>));
        services.Remove(toursDescriptor!);
        services.AddDbContext<ToursContext>(SetupTestContext());

        var paymentsDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<PaymentsContext>));
        services.Remove(paymentsDescriptor!);
        services.AddDbContext<PaymentsContext>(SetupTestContext());

        var encounterDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<EncountersContext>));
        services.Remove(encounterDescriptor!);
        services.AddDbContext<EncountersContext>(SetupTestContext());

        var stakeholdersDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<StakeholdersContext>));
        services.Remove(stakeholdersDescriptor!);
        services.AddDbContext<StakeholdersContext>(SetupTestContext());


        services.AddAuthentication("TestAuth")
           .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>("TestAuth", _ => { });

        services.AddScoped<IAchievementService, AchievementService>();
        services.AddScoped<INotificationService, NotificationService>();


        return services;
    }
}
