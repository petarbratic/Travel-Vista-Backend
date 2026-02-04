
using Explorer.Blog.Infrastructure.Database;
using Explorer.BuildingBlocks.Tests;
using Explorer.Payments.Infrastructure.Database;
using Explorer.Stakeholders.API.Dtos;
using Explorer.Stakeholders.API.Internal;
using Explorer.Stakeholders.Infrastructure.Database;
using Explorer.Tours.API.Dtos;
using Explorer.Tours.API.Internal;
using Explorer.Tours.Infrastructure.Database;
using Explorer.Encounters.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Explorer.Payments.Tests
{
    public class PaymentsTestFactory : BaseTestFactory<PaymentsContext>
    {
        protected override IServiceCollection ReplaceNeededDbContexts(IServiceCollection services)
        {
            var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<PaymentsContext>));
            services.Remove(descriptor!);
            services.AddDbContext<PaymentsContext>(SetupTestContext());

            var toursDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<ToursContext>));
            services.Remove(toursDescriptor!);
            services.AddDbContext<ToursContext>(SetupTestContext());

            var stakeholdersDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<StakeholdersContext>));
            services.Remove(stakeholdersDescriptor!);
            services.AddDbContext<StakeholdersContext>(SetupTestContext());

            var blogsDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<BlogContext>));
            services.Remove(blogsDescriptor!);
            services.AddDbContext<BlogContext>(SetupTestContext());

            var encountersDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<EncountersContext>));
            services.Remove(encountersDescriptor!);
            services.AddDbContext<EncountersContext>(SetupTestContext());

            // ==================== MOCK: IInternalTourService ====================
            var existingInternalTour = services.FirstOrDefault(d => d.ServiceType == typeof(IInternalTourService));
            if (existingInternalTour != null) services.Remove(existingInternalTour);

            var internalTourMock = new Mock<IInternalTourService>();

            // Tour -2: Published, 500 AC (OK za kupovinu)
            internalTourMock.Setup(s => s.GetById(-2)).Returns(new TourDto
            {
                Id = -2,
                Name = "Test Tour -2",
                Price = 500m,
                Status = (int)TourStatusDto.Published,
                ArchivedAt = null
            });

            // Tour -4: Published, 700 AC (OK za kupovinu)
            internalTourMock.Setup(s => s.GetById(-4)).Returns(new TourDto
            {
                Id = -4,
                Name = "Test Tour -4",
                Price = 700m,
                Status = (int)TourStatusDto.Published,
                ArchivedAt = null
            });

            // Tour -3: Archived (NE MOŽE se kupiti)
            internalTourMock.Setup(s => s.GetById(-3)).Returns(new TourDto
            {
                Id = -3,
                Name = "Archived Tour",
                Price = 300m,
                Status = (int)TourStatusDto.Published,
                ArchivedAt = DateTime.UtcNow.AddDays(-10) // ARHIVIRANA
            });

            // Tour -1: Draft (NE MOŽE se kupiti)
            internalTourMock.Setup(s => s.GetById(-1)).Returns(new TourDto
            {
                Id = -1,
                Name = "Draft Tour",
                Price = 200m,
                Status = (int)TourStatusDto.Draft, // DRAFT status
                ArchivedAt = null
            });

            // Tour -99: Ne postoji (vraća null)
            internalTourMock.Setup(s => s.GetById(-99)).Returns((TourDto)null);

            // Svi ostali ID-evi: vraća null (simulira nepostojanje)
            internalTourMock.Setup(s => s.GetById(It.Is<long>(id =>
                id != -2 && id != -3 && id != -4 && id != -1 && id < 0)))
                .Returns((TourDto)null);

            // Pozitivni ID-evi (dinamički kreirani u testovima): OK za kupovinu
            internalTourMock.Setup(s => s.GetById(It.Is<long>(id => id > 0)))
                .Returns((long tourId) => new TourDto
                {
                    Id = tourId,
                    Name = $"Dynamic Tour {tourId}",
                    Price = 500m,
                    Status = (int)TourStatusDto.Published,
                    ArchivedAt = null
                });

            // GetDiscountedPrice vraća originalnu cenu (nema popusta u testovima)
            internalTourMock.Setup(s => s.GetDiscountedPrice(It.IsAny<long>(), It.IsAny<decimal>()))
                .Returns<long, decimal>((tourId, originalPrice) => originalPrice);

            services.AddScoped<IInternalTourService>(_ => internalTourMock.Object);

            // ==================== MOCK: IInternalWalletService ====================
            var existingWallet = services.FirstOrDefault(d => d.ServiceType == typeof(IInternalWalletService));
            if (existingWallet != null) services.Remove(existingWallet);

            var walletMock = new Mock<IInternalWalletService>();
            // Koristimo static dictionary koji se deli između svih scope-ova
            // Resetuje se između testova jer se kreira novi Factory instance
            var walletBalances = new Dictionary<long, int>();

            walletMock.Setup(s => s.GetWallet(It.IsAny<long>()))
                .Returns((long personId) =>
                {
                    lock (walletBalances)
                    {
                        if (!walletBalances.ContainsKey(personId))
                            walletBalances[personId] = 100000;
                        return new WalletDto
                        {
                            PersonId = personId,
                            BalanceAc = walletBalances[personId]
                        };
                    }
                });                        

            walletMock.Setup(s => s.DeductAc(It.IsAny<long>(), It.IsAny<decimal>()))
                .Callback((long personId, decimal amount) =>
                {
                    lock (walletBalances)
                    {
                        if (!walletBalances.ContainsKey(personId))
                            walletBalances[personId] = 100000;
                        walletBalances[personId] -= (int)amount;
                    }
                })
                .Returns((long personId, decimal amount) =>
                {
                    lock (walletBalances)
                    {
                        if (!walletBalances.ContainsKey(personId))
                            walletBalances[personId] = 100000;
                        return new WalletDto
                        {
                            PersonId = personId,
                            BalanceAc = walletBalances[personId]
                        };
                    }
                });

            services.AddSingleton<IInternalWalletService>(_ => walletMock.Object);

            // ==================== MOCK: IInternalNotificationService ====================
            var existingNotification = services.FirstOrDefault(d => d.ServiceType == typeof(IInternalNotificationService));
            if (existingNotification != null) services.Remove(existingNotification);

            var notificationMock = new Mock<IInternalNotificationService>();
            notificationMock.Setup(s => s.CreateTourPurchaseNotification(
                It.IsAny<long>(),
                It.IsAny<long>(),
                It.IsAny<string>()));

            services.AddScoped<IInternalNotificationService>(_ => notificationMock.Object);
            // ==================== MOCK: IInternalBundleService ====================
            var existingBundle = services.FirstOrDefault(d => d.ServiceType == typeof(IInternalBundleService));
            if (existingBundle != null) services.Remove(existingBundle);

            var bundleMock = new Mock<IInternalBundleService>();

            // Bundle -1: Published, 1000 AC, sadrži ture [-2, -4]
            bundleMock.Setup(s => s.GetById(-1)).Returns(new Tours.API.Dtos.BundleDto
            {
                Id = -1,
                Name = "Test Bundle -1",
                Price = 1000m,
                Status = 1, // Published
                AuthorId = -100,
                TourIds = new List<long> { -2, -4 },
                CreatedAt = DateTime.UtcNow.AddDays(-10)
            });

            // Bundle -2: Draft (NE MOŽE se kupiti)
            bundleMock.Setup(s => s.GetById(-2)).Returns(new Tours.API.Dtos.BundleDto
            {
                Id = -2,
                Name = "Draft Bundle",
                Price = 500m,
                Status = 0, // Draft
                AuthorId = -100,
                TourIds = new List<long> { -2 },
                CreatedAt = DateTime.UtcNow
            });

            // Bundle -3: Published ali JEFTINIJI (200 AC)
            bundleMock.Setup(s => s.GetById(-3)).Returns(new Tours.API.Dtos.BundleDto
            {
                Id = -3,
                Name = "Cheap Bundle",
                Price = 200m,
                Status = 1, // Published
                AuthorId = -100,
                TourIds = new List<long> { -2 },
                CreatedAt = DateTime.UtcNow
            });

            // Bundle -99: Ne postoji
            bundleMock.Setup(s => s.GetById(-99)).Returns((Tours.API.Dtos.BundleDto)null);

            services.AddScoped<IInternalBundleService>(_ => bundleMock.Object);

            // ==================== MOCK: IInternalXpEventService ====================
            var existingXp = services.FirstOrDefault(d => d.ServiceType == typeof(IInternalXpEventService));
            if (existingXp != null) services.Remove(existingXp);

            var xpMock = new Mock<IInternalXpEventService>();


            xpMock.Setup(x => x.BuyTourXp(It.IsAny<long>(), It.IsAny<long>(), It.IsAny<int>()));

            services.AddScoped<IInternalXpEventService>(_ => xpMock.Object);

            return services;
        }
    }
}