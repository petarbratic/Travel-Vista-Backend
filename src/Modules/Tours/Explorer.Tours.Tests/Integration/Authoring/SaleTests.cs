using Explorer.Tours.API.Dtos;
using Explorer.Tours.API.Public.Authoring;
using Explorer.Tours.Infrastructure.Database;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using System;
using System.Collections.Generic;
using Xunit;

namespace Explorer.Tours.Tests.Integration.Authoring
{
    [Collection("Sequential")]
    public class SaleTests : BaseToursIntegrationTest
    {
        public SaleTests(ToursTestFactory factory) : base(factory) { }

        [Fact]
        public void Creates_sale()
        {
            // Arrange
            using var scope = Factory.Services.CreateScope();
            var service = scope.ServiceProvider.GetRequiredService<ISaleService>();

            var saleDto = new SaleCreateDto
            {
                TourIds = new List<long> { -2, -4 }, // Published tours owned by author -11
                StartDate = DateTime.UtcNow.AddDays(1),
                EndDate = DateTime.UtcNow.AddDays(7),
                DiscountPercentage = 20
            };

            // Act
            var result = service.Create(saleDto, -11);

            // Assert
            result.ShouldNotBeNull();
            result.TourIds.Count.ShouldBe(2);
            result.DiscountPercentage.ShouldBe(20);
            result.AuthorId.ShouldBe(-11);
            result.StartDate.ShouldBeLessThan(result.EndDate);
        }

        [Fact]
        public void Creates_sale_fails_tour_not_found()
        {
            // Arrange
            using var scope = Factory.Services.CreateScope();
            var service = scope.ServiceProvider.GetRequiredService<ISaleService>();

            var saleDto = new SaleCreateDto
            {
                TourIds = new List<long> { -2, 999 }, // 999 doesn't exist
                StartDate = DateTime.UtcNow.AddDays(1),
                EndDate = DateTime.UtcNow.AddDays(7),
                DiscountPercentage = 15
            };

            // Act & Assert
            Should.Throw<Exception>(() => service.Create(saleDto, -11));
        }

        [Fact]
        public void Creates_sale_fails_tour_not_owned()
        {
            // Arrange
            using var scope = Factory.Services.CreateScope();
            var service = scope.ServiceProvider.GetRequiredService<ISaleService>();

            var saleDto = new SaleCreateDto
            {
                TourIds = new List<long> { -2 }, // This tour belongs to author -11
                StartDate = DateTime.UtcNow.AddDays(1),
                EndDate = DateTime.UtcNow.AddDays(7),
                DiscountPercentage = 10
            };

            // Act & Assert
            Should.Throw<Exception>(() => service.Create(saleDto, -12)); // Different author
        }

        [Fact]
        public void Creates_sale_fails_invalid_dates()
        {
            // Arrange
            using var scope = Factory.Services.CreateScope();
            var service = scope.ServiceProvider.GetRequiredService<ISaleService>();

            var saleDto = new SaleCreateDto
            {
                TourIds = new List<long> { -2 },
                StartDate = DateTime.UtcNow.AddDays(7),
                EndDate = DateTime.UtcNow.AddDays(1), // End before start
                DiscountPercentage = 15
            };

            // Act & Assert
            Should.Throw<ArgumentException>(() => service.Create(saleDto, -11));
        }

        [Fact]
        public void Creates_sale_fails_discount_over_100()
        {
            // Arrange
            using var scope = Factory.Services.CreateScope();
            var service = scope.ServiceProvider.GetRequiredService<ISaleService>();

            var saleDto = new SaleCreateDto
            {
                TourIds = new List<long> { -2 },
                StartDate = DateTime.UtcNow.AddDays(1),
                EndDate = DateTime.UtcNow.AddDays(7),
                DiscountPercentage = 150 // Invalid
            };

            // Act & Assert
            Should.Throw<ArgumentException>(() => service.Create(saleDto, -11));
        }

        [Fact]
        public void Creates_sale_fails_discount_negative()
        {
            // Arrange
            using var scope = Factory.Services.CreateScope();
            var service = scope.ServiceProvider.GetRequiredService<ISaleService>();

            var saleDto = new SaleCreateDto
            {
                TourIds = new List<long> { -2 },
                StartDate = DateTime.UtcNow.AddDays(1),
                EndDate = DateTime.UtcNow.AddDays(7),
                DiscountPercentage = -10 // Invalid
            };

            // Act & Assert
            Should.Throw<ArgumentException>(() => service.Create(saleDto, -11));
        }

        [Fact]
        public void Creates_sale_fails_end_date_more_than_2_weeks()
        {
            // Arrange
            using var scope = Factory.Services.CreateScope();
            var service = scope.ServiceProvider.GetRequiredService<ISaleService>();

            var saleDto = new SaleCreateDto
            {
                TourIds = new List<long> { -2 },
                StartDate = DateTime.UtcNow.AddDays(1),
                EndDate = DateTime.UtcNow.AddDays(20), // More than 2 weeks
                DiscountPercentage = 15
            };

            // Act & Assert
            Should.Throw<ArgumentException>(() => service.Create(saleDto, -11));
        }

        [Fact]
        public void Creates_sale_fails_empty_tour_list()
        {
            // Arrange
            using var scope = Factory.Services.CreateScope();
            var service = scope.ServiceProvider.GetRequiredService<ISaleService>();

            var saleDto = new SaleCreateDto
            {
                TourIds = new List<long>(), // Empty
                StartDate = DateTime.UtcNow.AddDays(1),
                EndDate = DateTime.UtcNow.AddDays(7),
                DiscountPercentage = 15
            };

            // Act & Assert
            Should.Throw<ArgumentException>(() => service.Create(saleDto, -11));
        }

        [Fact]
        public void Updates_sale()
        {
            // Arrange
            using var scope = Factory.Services.CreateScope();
            var service = scope.ServiceProvider.GetRequiredService<ISaleService>();

            var createDto = new SaleCreateDto
            {
                TourIds = new List<long> { -2 },
                StartDate = DateTime.UtcNow.AddDays(1),
                EndDate = DateTime.UtcNow.AddDays(7),
                DiscountPercentage = 10
            };
            var created = service.Create(createDto, -11);

            var updateDto = new SaleUpdateDto
            {
                Id = created.Id,
                TourIds = new List<long> { -2, -4 },
                StartDate = DateTime.UtcNow.AddDays(2),
                EndDate = DateTime.UtcNow.AddDays(8),
                DiscountPercentage = 20
            };

            // Act
            var result = service.Update(updateDto, -11);

            // Assert
            result.TourIds.Count.ShouldBe(2);
            result.DiscountPercentage.ShouldBe(20);
            result.UpdatedAt.ShouldNotBeNull();
        }

        [Fact]
        public void Updates_sale_fails_not_owner()
        {
            // Arrange
            using var scope = Factory.Services.CreateScope();
            var service = scope.ServiceProvider.GetRequiredService<ISaleService>();

            var createDto = new SaleCreateDto
            {
                TourIds = new List<long> { -2 },
                StartDate = DateTime.UtcNow.AddDays(1),
                EndDate = DateTime.UtcNow.AddDays(7),
                DiscountPercentage = 10
            };
            var created = service.Create(createDto, -11);

            var updateDto = new SaleUpdateDto
            {
                Id = created.Id,
                TourIds = new List<long> { -2 },
                StartDate = DateTime.UtcNow.AddDays(2),
                EndDate = DateTime.UtcNow.AddDays(8),
                DiscountPercentage = 20
            };

            // Act & Assert
            Should.Throw<Exception>(() => service.Update(updateDto, -12)); // Different author
        }

        [Fact]
        public void Updates_sale_fails_not_found()
        {
            // Arrange
            using var scope = Factory.Services.CreateScope();
            var service = scope.ServiceProvider.GetRequiredService<ISaleService>();

            var updateDto = new SaleUpdateDto
            {
                Id = 99999, // Doesn't exist
                TourIds = new List<long> { -2 },
                StartDate = DateTime.UtcNow.AddDays(1),
                EndDate = DateTime.UtcNow.AddDays(7),
                DiscountPercentage = 15
            };

            // Act & Assert
            Should.Throw<Exception>(() => service.Update(updateDto, -11));
        }

        [Fact]
        public void Deletes_sale()
        {
            // Arrange
            using var scope = Factory.Services.CreateScope();
            var service = scope.ServiceProvider.GetRequiredService<ISaleService>();

            var createDto = new SaleCreateDto
            {
                TourIds = new List<long> { -2 },
                StartDate = DateTime.UtcNow.AddDays(1),
                EndDate = DateTime.UtcNow.AddDays(7),
                DiscountPercentage = 10
            };
            var created = service.Create(createDto, -11);

            // Act
            service.Delete(created.Id, -11);

            // Assert
            Should.Throw<Exception>(() => service.GetById(created.Id));
        }

        [Fact]
        public void Deletes_sale_fails_not_owner()
        {
            // Arrange
            using var scope = Factory.Services.CreateScope();
            var service = scope.ServiceProvider.GetRequiredService<ISaleService>();

            var createDto = new SaleCreateDto
            {
                TourIds = new List<long> { -2 },
                StartDate = DateTime.UtcNow.AddDays(1),
                EndDate = DateTime.UtcNow.AddDays(7),
                DiscountPercentage = 10
            };
            var created = service.Create(createDto, -11);

            // Act & Assert
            Should.Throw<Exception>(() => service.Delete(created.Id, -12)); // Different author
        }

        [Fact]
        public void Deletes_sale_fails_not_found()
        {
            // Arrange
            using var scope = Factory.Services.CreateScope();
            var service = scope.ServiceProvider.GetRequiredService<ISaleService>();

            // Act & Assert
            Should.Throw<Exception>(() => service.Delete(99999, -11));
        }

        [Fact]
        public void Gets_sale_by_id()
        {
            // Arrange
            using var scope = Factory.Services.CreateScope();
            var service = scope.ServiceProvider.GetRequiredService<ISaleService>();

            var createDto = new SaleCreateDto
            {
                TourIds = new List<long> { -2, -4 },
                StartDate = DateTime.UtcNow.AddDays(1),
                EndDate = DateTime.UtcNow.AddDays(7),
                DiscountPercentage = 25
            };
            var created = service.Create(createDto, -11);

            // Act
            var result = service.GetById(created.Id);

            // Assert
            result.ShouldNotBeNull();
            result.Id.ShouldBe(created.Id);
            result.TourIds.Count.ShouldBe(2);
            result.DiscountPercentage.ShouldBe(25);
            result.AuthorId.ShouldBe(-11);
        }

        [Fact]
        public void Gets_sale_by_id_fails_not_found()
        {
            // Arrange
            using var scope = Factory.Services.CreateScope();
            var service = scope.ServiceProvider.GetRequiredService<ISaleService>();

            // Act & Assert
            Should.Throw<Exception>(() => service.GetById(99999));
        }

        [Fact]
        public void Gets_sales_by_author_id()
        {
            // Arrange
            using var scope = Factory.Services.CreateScope();
            var service = scope.ServiceProvider.GetRequiredService<ISaleService>();

            var createDto1 = new SaleCreateDto
            {
                TourIds = new List<long> { -2 },
                StartDate = DateTime.UtcNow.AddDays(1),
                EndDate = DateTime.UtcNow.AddDays(7),
                DiscountPercentage = 10
            };
            service.Create(createDto1, -11);

            var createDto2 = new SaleCreateDto
            {
                TourIds = new List<long> { -4 },
                StartDate = DateTime.UtcNow.AddDays(2),
                EndDate = DateTime.UtcNow.AddDays(8),
                DiscountPercentage = 15
            };
            service.Create(createDto2, -11);

            // Act
            var result = service.GetByAuthorId(-11);

            // Assert
            result.ShouldNotBeNull();
            result.Count.ShouldBeGreaterThanOrEqualTo(2);
        }

        [Fact]
        public void Gets_sales_by_author_id_returns_empty_for_different_author()
        {
            // Arrange
            using var scope = Factory.Services.CreateScope();
            var service = scope.ServiceProvider.GetRequiredService<ISaleService>();

            var createDto = new SaleCreateDto
            {
                TourIds = new List<long> { -2 },
                StartDate = DateTime.UtcNow.AddDays(1),
                EndDate = DateTime.UtcNow.AddDays(7),
                DiscountPercentage = 10
            };
            service.Create(createDto, -11);

            // Act
            var result = service.GetByAuthorId(-12); // Different author

            // Assert
            result.ShouldNotBeNull();
            // Should not contain sales from author -11
            result.ShouldNotContain(s => s.AuthorId == -11);
        }
    }
}
