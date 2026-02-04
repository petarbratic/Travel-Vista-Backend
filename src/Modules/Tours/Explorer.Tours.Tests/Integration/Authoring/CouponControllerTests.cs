using AuthorCouponController = Explorer.API.Controllers.Author.Authoring.CouponController;
using TouristCouponController = Explorer.API.Controllers.Tourist.CouponController;
using Explorer.Tours.API.Dtos;
using Explorer.Tours.API.Public.Authoring;
using Explorer.Tours.Infrastructure.Database;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Explorer.Tours.Tests.Integration.Authoring
{
    [Collection("Sequential")]
    public class CouponControllerTests : BaseToursIntegrationTest
    {
        public CouponControllerTests(ToursTestFactory factory) : base(factory) { }

        private const string TestAuthorId = "-11";
        private const string TestTouristId = "-21";
        private const long TestTourId = -2; // Published tour owned by author -11

        private AuthorCouponController CreateAuthorCouponController(IServiceScope scope, string personId)
        {
            return new AuthorCouponController(
                scope.ServiceProvider.GetRequiredService<ICouponService>())
            {
                ControllerContext = BuildContext(personId)
            };
        }

        private TouristCouponController CreateTouristCouponController(IServiceScope scope, string personId)
        {
            return new TouristCouponController(
                scope.ServiceProvider.GetRequiredService<ICouponService>())
            {
                ControllerContext = BuildContext(personId)
            };
        }

        [Fact]
        public void Author_creates_coupon_successfully()
        {
            // Arrange
            using var scope = Factory.Services.CreateScope();
            var controller = CreateAuthorCouponController(scope, TestAuthorId);

            var couponDto = new CouponCreateDto
            {
                DiscountPercentage = 15,
                ExpiryDate = DateTime.UtcNow.AddDays(30),
                TourId = TestTourId
            };

            // Act
            var result = controller.Create(couponDto);

            // Assert
            result.ShouldNotBeNull();
            var okResult = result.Result.ShouldBeOfType<CreatedAtActionResult>();
            var coupon = okResult.Value.ShouldBeOfType<CouponDto>();
            coupon.DiscountPercentage.ShouldBe(15);
            coupon.TourId.ShouldBe(TestTourId);
            coupon.AuthorId.ShouldBe(long.Parse(TestAuthorId));
            coupon.Code.ShouldNotBeNullOrEmpty();
            coupon.Code.Length.ShouldBe(8);
        }

        [Fact]
        public void Author_creates_coupon_for_all_tours()
        {
            // Arrange
            using var scope = Factory.Services.CreateScope();
            var controller = CreateAuthorCouponController(scope, TestAuthorId);

            var couponDto = new CouponCreateDto
            {
                DiscountPercentage = 20,
                ExpiryDate = null, // No expiration
                TourId = null // For all tours
            };

            // Act
            var result = controller.Create(couponDto);

            // Assert
            result.ShouldNotBeNull();
            var okResult = result.Result.ShouldBeOfType<CreatedAtActionResult>();
            var coupon = okResult.Value.ShouldBeOfType<CouponDto>();
            coupon.DiscountPercentage.ShouldBe(20);
            coupon.TourId.ShouldBeNull();
            coupon.ExpiryDate.ShouldBeNull();
        }

        [Fact]
        public void Author_gets_my_coupons()
        {
            // Arrange
            using var scope = Factory.Services.CreateScope();
            var controller = CreateAuthorCouponController(scope, TestAuthorId);

            // Create a coupon first
            var createDto = new CouponCreateDto
            {
                DiscountPercentage = 10,
                TourId = TestTourId
            };
            controller.Create(createDto);

            // Act
            var result = controller.GetMyCoupons();

            // Assert
            result.ShouldNotBeNull();
            var okResult = result.Result.ShouldBeOfType<OkObjectResult>();
            var coupons = okResult.Value.ShouldBeOfType<List<CouponDto>>();
            coupons.ShouldNotBeNull();
            coupons.Count.ShouldBeGreaterThan(0);
            coupons.ShouldContain(c => c.AuthorId == long.Parse(TestAuthorId));
        }

        [Fact]
        public void Author_gets_coupon_by_id()
        {
            // Arrange
            using var scope = Factory.Services.CreateScope();
            var controller = CreateAuthorCouponController(scope, TestAuthorId);

            var createDto = new CouponCreateDto
            {
                DiscountPercentage = 25,
                TourId = TestTourId
            };
            var createResult = controller.Create(createDto);
            var createdCoupon = (createResult.Result as CreatedAtActionResult)?.Value as CouponDto;
            createdCoupon.ShouldNotBeNull();

            // Act
            var result = controller.GetById(createdCoupon.Id);

            // Assert
            result.ShouldNotBeNull();
            var okResult = result.Result.ShouldBeOfType<OkObjectResult>();
            var coupon = okResult.Value.ShouldBeOfType<CouponDto>();
            coupon.Id.ShouldBe(createdCoupon.Id);
            coupon.DiscountPercentage.ShouldBe(25);
            coupon.Code.ShouldBe(createdCoupon.Code);
        }

        [Fact]
        public void Author_updates_coupon()
        {
            // Arrange
            using var scope = Factory.Services.CreateScope();
            var controller = CreateAuthorCouponController(scope, TestAuthorId);

            var createDto = new CouponCreateDto
            {
                DiscountPercentage = 10,
                TourId = TestTourId
            };
            var createResult = controller.Create(createDto);
            var createdCoupon = (createResult.Result as CreatedAtActionResult)?.Value as CouponDto;

            var updateDto = new CouponUpdateDto
            {
                DiscountPercentage = 30,
                ExpiryDate = DateTime.UtcNow.AddDays(60),
                TourId = TestTourId
            };

            // Act
            var result = controller.Update(createdCoupon.Id, updateDto);

            // Assert
            result.ShouldNotBeNull();
            var okResult = result.Result.ShouldBeOfType<OkObjectResult>();
            var updatedCoupon = okResult.Value.ShouldBeOfType<CouponDto>();
            updatedCoupon.Id.ShouldBe(createdCoupon.Id);
            updatedCoupon.DiscountPercentage.ShouldBe(30);
            updatedCoupon.Code.ShouldBe(createdCoupon.Code); // Code shouldn't change
        }

        [Fact]
        public void Author_deletes_coupon()
        {
            // Arrange
            using var scope = Factory.Services.CreateScope();
            var controller = CreateAuthorCouponController(scope, TestAuthorId);

            var createDto = new CouponCreateDto
            {
                DiscountPercentage = 15,
                TourId = TestTourId
            };
            var createResult = controller.Create(createDto);
            var createdCoupon = (createResult.Result as CreatedAtActionResult)?.Value as CouponDto;

            // Act
            var result = controller.Delete(createdCoupon.Id);

            // Assert
            result.ShouldNotBeNull();
            result.ShouldBeOfType<NoContentResult>();

            // Verify it's deleted
            Should.Throw<Exception>(() => controller.GetById(createdCoupon.Id));
        }

        [Fact]
        public void Tourist_validates_coupon_for_single_tour()
        {
            // Arrange
            using var scope = Factory.Services.CreateScope();
            var authorController = CreateAuthorCouponController(scope, TestAuthorId);
            var touristController = CreateTouristCouponController(scope, TestTouristId);

            // Create a coupon for specific tour
            var createDto = new CouponCreateDto
            {
                DiscountPercentage = 20,
                TourId = TestTourId // Tour -2 belongs to author -11
            };
            var createResult = authorController.Create(createDto);
            var coupon = (createResult.Result as CreatedAtActionResult)?.Value as CouponDto;
            coupon.ShouldNotBeNull();

            var validationDto = new CouponValidationDto
            {
                Code = coupon.Code,
                TourId = TestTourId,
                TourIds = null // Don't send TourIds to use single tour validation
            };

            // Act
            var result = touristController.ValidateCoupon(validationDto);

            // Assert
            result.ShouldNotBeNull();
            var okResult = result.Result.ShouldBeOfType<OkObjectResult>();
            var validationResult = okResult.Value.ShouldBeOfType<CouponValidationResultDto>();
            validationResult.IsValid.ShouldBeTrue();
            validationResult.DiscountPercentage.ShouldBe(20);
            validationResult.DiscountAmount.ShouldBeGreaterThan(0);
            validationResult.AppliedToTourId.ShouldBe(TestTourId);
            validationResult.OriginalPrice.ShouldBeGreaterThan(0);
            validationResult.FinalPrice.ShouldBeLessThan(validationResult.OriginalPrice);
        }

        [Fact]
        public void Tourist_validates_coupon_for_cart()
        {
            // Arrange
            using var scope = Factory.Services.CreateScope();
            var authorController = CreateAuthorCouponController(scope, TestAuthorId);
            var touristController = CreateTouristCouponController(scope, TestTouristId);

            // Create a coupon for all tours
            var createDto = new CouponCreateDto
            {
                DiscountPercentage = 15,
                TourId = null // For all tours
            };
            var createResult = authorController.Create(createDto);
            var coupon = (createResult.Result as CreatedAtActionResult)?.Value as CouponDto;

            var validationDto = new CouponValidationDto
            {
                Code = coupon.Code,
                TourId = TestTourId,
                TourIds = new List<long> { TestTourId, -4 }, // Multiple tours
                TourPrices = new Dictionary<long, decimal>
                {
                    { TestTourId, 500m },
                    { -4, 300m }
                }
            };

            // Act
            var result = touristController.ValidateCoupon(validationDto);

            // Assert
            result.ShouldNotBeNull();
            var okResult = result.Result.ShouldBeOfType<OkObjectResult>();
            var validationResult = okResult.Value.ShouldBeOfType<CouponValidationResultDto>();
            validationResult.IsValid.ShouldBeTrue();
            validationResult.DiscountPercentage.ShouldBe(15);
            validationResult.DiscountAmount.ShouldBeGreaterThan(0);
        }

        [Fact]
        public void Tourist_validates_coupon_with_discounted_price()
        {
            // Arrange
            using var scope = Factory.Services.CreateScope();
            var authorController = CreateAuthorCouponController(scope, TestAuthorId);
            var touristController = CreateTouristCouponController(scope, TestTouristId);

            // Create a coupon
            var createDto = new CouponCreateDto
            {
                DiscountPercentage = 10,
                TourId = TestTourId
            };
            var createResult = authorController.Create(createDto);
            var coupon = (createResult.Result as CreatedAtActionResult)?.Value as CouponDto;

            // Tour original price is 850, but discounted to 425 (50% sale)
            var discountedPrice = 425m;
            var validationDto = new CouponValidationDto
            {
                Code = coupon.Code,
                TourId = TestTourId,
                TourIds = new List<long> { TestTourId },
                TourPrices = new Dictionary<long, decimal>
                {
                    { TestTourId, discountedPrice }
                }
            };

            // Act
            var result = touristController.ValidateCoupon(validationDto);

            // Assert
            result.ShouldNotBeNull();
            var okResult = result.Result.ShouldBeOfType<OkObjectResult>();
            var validationResult = okResult.Value.ShouldBeOfType<CouponValidationResultDto>();
            validationResult.IsValid.ShouldBeTrue();
            validationResult.OriginalPrice.ShouldBe(discountedPrice); // Should use discounted price from cart
            validationResult.DiscountAmount.ShouldBe(discountedPrice * 0.10m); // 10% of discounted price = 42.5
            validationResult.FinalPrice.ShouldBe(discountedPrice - validationResult.DiscountAmount);
        }

        [Fact]
        public void Tourist_validates_coupon_fails_invalid_code()
        {
            // Arrange
            using var scope = Factory.Services.CreateScope();
            var touristController = CreateTouristCouponController(scope, TestTouristId);

            var validationDto = new CouponValidationDto
            {
                Code = "INVALID123",
                TourId = TestTourId
            };

            // Act
            var result = touristController.ValidateCoupon(validationDto);

            // Assert
            result.ShouldNotBeNull();
            var okResult = result.Result.ShouldBeOfType<OkObjectResult>();
            var validationResult = okResult.Value.ShouldBeOfType<CouponValidationResultDto>();
            validationResult.IsValid.ShouldBeFalse();
            validationResult.Message.ShouldContain("Invalid");
        }


        [Fact]
        public void Tourist_validates_coupon_fails_wrong_tour()
        {
            // Arrange
            using var scope = Factory.Services.CreateScope();
            var authorController = CreateAuthorCouponController(scope, TestAuthorId);
            var touristController = CreateTouristCouponController(scope, TestTouristId);

            // Create coupon for specific tour
            var createDto = new CouponCreateDto
            {
                DiscountPercentage = 15,
                TourId = TestTourId
            };
            var createResult = authorController.Create(createDto);
            var coupon = (createResult.Result as CreatedAtActionResult)?.Value as CouponDto;

            // Try to validate for different tour
            var validationDto = new CouponValidationDto
            {
                Code = coupon.Code,
                TourId = -4 // Different tour
            };

            // Act
            var result = touristController.ValidateCoupon(validationDto);

            // Assert
            result.ShouldNotBeNull();
            var okResult = result.Result.ShouldBeOfType<OkObjectResult>();
            var validationResult = okResult.Value.ShouldBeOfType<CouponValidationResultDto>();
            validationResult.IsValid.ShouldBeFalse();
            validationResult.Message.ShouldContain("not valid");
        }

        [Fact]
        public void Tourist_validates_coupon_fails_empty_cart()
        {
            // Arrange
            using var scope = Factory.Services.CreateScope();
            var authorController = CreateAuthorCouponController(scope, TestAuthorId);
            var touristController = CreateTouristCouponController(scope, TestTouristId);

            var createDto = new CouponCreateDto
            {
                DiscountPercentage = 10,
                TourId = null // For all tours
            };
            var createResult = authorController.Create(createDto);
            var coupon = (createResult.Result as CreatedAtActionResult)?.Value as CouponDto;
            coupon.ShouldNotBeNull();

            // Send empty TourIds list - kontroler će pozvati ValidateCouponForCart
            // koji će vratiti grešku jer je lista prazna
            var validationDto = new CouponValidationDto
            {
                Code = coupon.Code,
                TourId = TestTourId,
                TourIds = new List<long>() // Empty cart - kontroler će pozvati ValidateCouponForCart
            };

            // Act
            var result = touristController.ValidateCoupon(validationDto);

            // Assert
            result.ShouldNotBeNull();
            var okResult = result.Result.ShouldBeOfType<OkObjectResult>();
            var validationResult = okResult.Value.ShouldBeOfType<CouponValidationResultDto>();
            validationResult.IsValid.ShouldBeFalse();
            validationResult.Message.ToLower().ShouldContain("empty");
        }

        [Fact]
        public void Author_creates_coupon_fails_invalid_discount()
        {
            // Arrange
            using var scope = Factory.Services.CreateScope();
            var controller = CreateAuthorCouponController(scope, TestAuthorId);

            var couponDto = new CouponCreateDto
            {
                DiscountPercentage = 150, // Invalid - over 100%
                TourId = TestTourId
            };

            // Act & Assert
            Should.Throw<ArgumentException>(() => controller.Create(couponDto));
        }

        [Fact]
        public void Author_creates_coupon_fails_not_owner()
        {
            // Arrange
            using var scope = Factory.Services.CreateScope();
            var controller = CreateAuthorCouponController(scope, "-12"); // Different author

            var couponDto = new CouponCreateDto
            {
                DiscountPercentage = 15,
                TourId = TestTourId // Tour belongs to author -11
            };

            // Act & Assert
            Should.Throw<Exception>(() => controller.Create(couponDto));
        }
    }
}
