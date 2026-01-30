using Explorer.API.Controllers.Author_Tourist;
using Explorer.Blog.API.Dtos;
using Explorer.Blog.API.Public;
using Explorer.Stakeholders.API.Public;
using Explorer.Tours.API.Public;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using Xunit;

namespace Explorer.Blog.Tests.Integration
{
    [Collection("Sequential")]
    public class BlogStatusTests : BaseBlogIntegrationTest
    {
        public BlogStatusTests(BlogTestFactory factory) : base(factory) { }

        private void AttachUser(BlogController controller, int userId)
        {
            var claims = new List<Claim>
            {
                new Claim("id", userId.ToString()),
                new Claim("personId", userId.ToString()),
                new Claim(ClaimTypes.Role, "author")
            };

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuth")) }
            };
        }

        [Fact]
        public void ChangeStatus_to_published_succeeds()
        {
            using var scope = Factory.Services.CreateScope();
            var controller = CreateController(scope);
            AttachUser(controller, -11);

            var blog = controller.CreateBlog(new BlogDto
            {
                Title = "Status Test Blog",
                Description = "Test status change"
            });

            var createdBlog = ((CreatedAtActionResult)blog.Result).Value as BlogDto;

            var result = controller.ChangeStatus(createdBlog.Id, 1);

            result.Result.ShouldBeOfType<OkObjectResult>();
            var updated = ((OkObjectResult)result.Result).Value as BlogDto;
            updated.Status.ShouldBe(1);
        }

        [Fact]
        public void ChangeStatus_to_archived_succeeds()
        {
            using var scope = Factory.Services.CreateScope();
            var controller = CreateController(scope);
            AttachUser(controller, -11);

            var blog = controller.CreateBlog(new BlogDto
            {
                Title = "Archive Test",
                Description = "Will be archived"
            });

            var createdBlog = ((CreatedAtActionResult)blog.Result).Value as BlogDto;

            var result = controller.ChangeStatus(createdBlog.Id, 2);

            result.Result.ShouldBeOfType<OkObjectResult>();
            var updated = ((OkObjectResult)result.Result).Value as BlogDto;
            updated.Status.ShouldBe(2);
        }

        [Fact]
        public void ChangeStatus_fails_for_non_owner()
        {
            using var scope = Factory.Services.CreateScope();
            var controller = CreateController(scope);
            AttachUser(controller, -11);

            var blog = controller.CreateBlog(new BlogDto
            {
                Title = "Forbidden Status Change",
                Description = "Test"
            });

            var createdBlog = ((CreatedAtActionResult)blog.Result).Value as BlogDto;

            AttachUser(controller, -12);

            var result = controller.ChangeStatus(createdBlog.Id, 1);

            result.Result.ShouldBeOfType<ForbidResult>();
        }

        [Fact]
        public void ChangeStatus_with_invalid_status_fails()
        {
            using var scope = Factory.Services.CreateScope();
            var controller = CreateController(scope);
            AttachUser(controller, -11);

            var blog = controller.CreateBlog(new BlogDto
            {
                Title = "Invalid Status",
                Description = "Test"
            });

            var createdBlog = ((CreatedAtActionResult)blog.Result).Value as BlogDto;

            var result = controller.ChangeStatus(createdBlog.Id, 99);

            result.Result.ShouldBeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public void GetAllBlogs_returns_only_published()
        {
            using var scope = Factory.Services.CreateScope();
            var controller = CreateController(scope);
            AttachUser(controller, -11);

            // Kreiraj draft
            controller.CreateBlog(new BlogDto
            {
                Title = "Draft Blog",
                Description = "Should not appear"
            });

            // Kreiraj published
            var publishedBlog = controller.CreateBlog(new BlogDto
            {
                Title = "Published Blog",
                Description = "Should appear"
            });

            var createdBlog = ((CreatedAtActionResult)publishedBlog.Result).Value as BlogDto;
            controller.ChangeStatus(createdBlog.Id, 1);

            var result = controller.GetAllBlogs();

            result.Result.ShouldBeOfType<OkObjectResult>();
            var blogs = ((OkObjectResult)result.Result).Value as List<BlogDto>;

            blogs.ShouldNotBeEmpty();
            blogs.ShouldNotContain(b => b.Title == "Draft Blog");
            blogs.ShouldContain(b => b.Title == "Published Blog");
        }

        [Fact]
        public void GetBlogById_owner_can_see_draft()
        {
            using var scope = Factory.Services.CreateScope();
            var controller = CreateController(scope);
            AttachUser(controller, -11);

            var blog = controller.CreateBlog(new BlogDto
            {
                Title = "Owner Draft",
                Description = "Test"
            });

            var createdBlog = ((CreatedAtActionResult)blog.Result).Value as BlogDto;

            var result = controller.GetBlogById(createdBlog.Id);

            result.Result.ShouldBeOfType<OkObjectResult>();
            var retrieved = ((OkObjectResult)result.Result).Value as BlogDto;
            retrieved.Title.ShouldBe("Owner Draft");
        }

        [Fact]
        public void GetBlogById_non_owner_cannot_see_draft()
        {
            using var scope = Factory.Services.CreateScope();
            var controller = CreateController(scope);
            AttachUser(controller, -11);

            var blog = controller.CreateBlog(new BlogDto
            {
                Title = "Private Draft",
                Description = "Test"
            });

            var createdBlog = ((CreatedAtActionResult)blog.Result).Value as BlogDto;

            AttachUser(controller, -12);

            var result = controller.GetBlogById(createdBlog.Id);

            result.Result.ShouldBeOfType<NotFoundObjectResult>();
        }

        private static BlogController CreateController(IServiceScope scope)
        {
            return new BlogController(
                scope.ServiceProvider.GetRequiredService<IBlogService>(),
                scope.ServiceProvider.GetRequiredService<IAchievementService>(),
                scope.ServiceProvider.GetRequiredService<INotificationService>()
            );
        }
    }
}