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
using System.Security.Claims;
using Xunit;

namespace Explorer.Blog.Tests.Integration
{
    [Collection("Sequential")]
    public class BlogCommentIntegrationTests : BaseBlogIntegrationTest
    {
        public BlogCommentIntegrationTests(BlogTestFactory factory) : base(factory) { }

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
        public void AddComment_succeeds_on_published_blog()
        {
            using var scope = Factory.Services.CreateScope();
            var controller = CreateController(scope);
            AttachUser(controller, -11);

            var blog = controller.CreateBlog(new BlogDto
            {
                Title = "Comment Test Blog",
                Description = "Test comments"
            });

            var createdBlog = ((CreatedAtActionResult)blog.Result).Value as BlogDto;
            controller.ChangeStatus(createdBlog.Id, 1);

            var service = scope.ServiceProvider.GetRequiredService<IBlogService>();
            var comment = service.AddComment(createdBlog.Id, -12, "Test comment");

            comment.ShouldNotBeNull();
            comment.Text.ShouldBe("Test comment");
            comment.AuthorId.ShouldBe(-12);
        }

        [Fact]
        public void EditComment_succeeds_for_author()
        {
            using var scope = Factory.Services.CreateScope();
            var controller = CreateController(scope);
            AttachUser(controller, -11);

            var blog = controller.CreateBlog(new BlogDto
            {
                Title = "Edit Comment Blog",
                Description = "Test"
            });

            var createdBlog = ((CreatedAtActionResult)blog.Result).Value as BlogDto;
            controller.ChangeStatus(createdBlog.Id, 1);

            var service = scope.ServiceProvider.GetRequiredService<IBlogService>();
            var comment = service.AddComment(createdBlog.Id, -12, "Original");

            var edited = service.EditComment(createdBlog.Id, comment.Id, -12, "Edited");

            edited.Text.ShouldBe("Edited");
            edited.EditedAt.ShouldNotBeNull();
        }

        [Fact]
        public void DeleteComment_succeeds_for_author()
        {
            using var scope = Factory.Services.CreateScope();
            var controller = CreateController(scope);
            AttachUser(controller, -11);

            var blog = controller.CreateBlog(new BlogDto
            {
                Title = "Delete Comment Blog",
                Description = "Test"
            });

            var createdBlog = ((CreatedAtActionResult)blog.Result).Value as BlogDto;
            controller.ChangeStatus(createdBlog.Id, 1);

            var service = scope.ServiceProvider.GetRequiredService<IBlogService>();
            var comment = service.AddComment(createdBlog.Id, -12, "To delete");

            service.DeleteComment(createdBlog.Id, comment.Id, -12);

            var comments = service.GetComments(createdBlog.Id);
            comments.ShouldBeEmpty();
        }

        [Fact]
        public void GetComments_returns_all_comments()
        {
            using var scope = Factory.Services.CreateScope();
            var controller = CreateController(scope);
            AttachUser(controller, -11);

            var blog = controller.CreateBlog(new BlogDto
            {
                Title = "Multiple Comments",
                Description = "Test"
            });

            var createdBlog = ((CreatedAtActionResult)blog.Result).Value as BlogDto;
            controller.ChangeStatus(createdBlog.Id, 1);

            var service = scope.ServiceProvider.GetRequiredService<IBlogService>();
            service.AddComment(createdBlog.Id, -12, "First");
            service.AddComment(createdBlog.Id, -13, "Second");
            service.AddComment(createdBlog.Id, -14, "Third");

            var comments = service.GetComments(createdBlog.Id);

            comments.Count.ShouldBe(3);
            comments[0].Text.ShouldBe("First");
            comments[1].Text.ShouldBe("Second");
            comments[2].Text.ShouldBe("Third");
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