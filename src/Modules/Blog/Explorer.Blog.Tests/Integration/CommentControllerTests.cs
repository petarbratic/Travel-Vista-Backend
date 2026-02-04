using Explorer.API.Controllers.Author_Tourist;
using Explorer.Blog.API.Dtos;
using Explorer.Blog.API.Public;
using Explorer.Stakeholders.API.Public;
using Explorer.Tours.API.Public;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using Xunit;

namespace Explorer.Blog.Tests.Integration
{
    [Collection("Sequential")]
    public class CommentControllerTests : BaseBlogIntegrationTest
    {
        public CommentControllerTests(BlogTestFactory factory) : base(factory) { }

        private void AttachUser(ControllerBase controller, int userId)
        {
            var claims = new List<Claim>
            {
                new Claim("id", userId.ToString()),
                new Claim("personId", userId.ToString())
            };

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuth")) }
            };
        }

        [Fact]
        public void AddComment_via_controller_succeeds()
        {
            using var scope = Factory.Services.CreateScope();
            var blogController = CreateBlogController(scope);
            AttachUser(blogController, -11);

            var blog = blogController.CreateBlog(new BlogDto
            {
                Title = "Comment Controller Test",
                Description = "Test"
            });

            var createdBlog = ((CreatedAtActionResult)blog.Result).Value as BlogDto;
            blogController.ChangeStatus(createdBlog.Id, 1);

            var commentController = CreateCommentController(scope);
            AttachUser(commentController, -12);

            var result = commentController.AddComment(createdBlog.Id, new CommentCreateDto { Text = "Test via controller" });

            result.Result.ShouldBeOfType<OkObjectResult>();
            var comment = ((OkObjectResult)result.Result).Value as CommentDto;
            comment.Text.ShouldBe("Test via controller");
        }

        [Fact]
        public void EditComment_via_controller_succeeds()
        {
            using var scope = Factory.Services.CreateScope();
            var blogController = CreateBlogController(scope);
            AttachUser(blogController, -11);

            var blog = blogController.CreateBlog(new BlogDto
            {
                Title = "Edit Comment Controller",
                Description = "Test"
            });

            var createdBlog = ((CreatedAtActionResult)blog.Result).Value as BlogDto;
            blogController.ChangeStatus(createdBlog.Id, 1);

            var commentController = CreateCommentController(scope);
            AttachUser(commentController, -12);

            var added = commentController.AddComment(createdBlog.Id, new CommentCreateDto { Text = "Original" });
            var addedComment = ((OkObjectResult)added.Result).Value as CommentDto;

            var result = commentController.EditComment(createdBlog.Id, addedComment.Id, new CommentCreateDto { Text = "Edited" });

            result.Result.ShouldBeOfType<OkObjectResult>();
            var edited = ((OkObjectResult)result.Result).Value as CommentDto;
            edited.Text.ShouldBe("Edited");
        }

        [Fact]
        public void DeleteComment_via_controller_succeeds()
        {
            using var scope = Factory.Services.CreateScope();
            var blogController = CreateBlogController(scope);
            AttachUser(blogController, -11);

            var blog = blogController.CreateBlog(new BlogDto
            {
                Title = "Delete Comment Controller",
                Description = "Test"
            });

            var createdBlog = ((CreatedAtActionResult)blog.Result).Value as BlogDto;
            blogController.ChangeStatus(createdBlog.Id, 1);

            var commentController = CreateCommentController(scope);
            AttachUser(commentController, -12);

            var added = commentController.AddComment(createdBlog.Id, new CommentCreateDto { Text = "To delete" });
            var addedComment = ((OkObjectResult)added.Result).Value as CommentDto;

            var result = commentController.DeleteComment(createdBlog.Id, addedComment.Id);

            result.ShouldBeOfType<NoContentResult>();
        }

        [Fact]
        public void GetComments_via_controller_returns_all()
        {
            using var scope = Factory.Services.CreateScope();
            var blogController = CreateBlogController(scope);
            AttachUser(blogController, -11);

            var blog = blogController.CreateBlog(new BlogDto
            {
                Title = "Get Comments Test",
                Description = "Test"
            });

            var createdBlog = ((CreatedAtActionResult)blog.Result).Value as BlogDto;
            blogController.ChangeStatus(createdBlog.Id, 1);

            var commentController = CreateCommentController(scope);
            AttachUser(commentController, -12);

            commentController.AddComment(createdBlog.Id, new CommentCreateDto { Text = "First" });
            commentController.AddComment(createdBlog.Id, new CommentCreateDto { Text = "Second" });

            var result = commentController.GetComments(createdBlog.Id);

            result.Result.ShouldBeOfType<OkObjectResult>();
            var comments = ((OkObjectResult)result.Result).Value as List<CommentDto>;
            comments.Count.ShouldBe(2);
        }

        [Fact]
        public void AddComment_with_empty_text_fails()
        {
            using var scope = Factory.Services.CreateScope();
            var service = scope.ServiceProvider.GetRequiredService<IBlogService>();

            var blog = service.CreateBlog(new BlogDto
            {
                Title = "Empty Comment Test",
                Description = "Test",
                AuthorId = -11
            });

            service.ChangeStatus(blog.Id, -11, 1);

            var ex = Should.Throw<ArgumentException>(() =>
                service.AddComment(blog.Id, -12, "")
            );

            ex.Message.ShouldContain("Text");
        }

        private static BlogController CreateBlogController(IServiceScope scope)
        {
            return new BlogController(
                scope.ServiceProvider.GetRequiredService<IBlogService>(),
                scope.ServiceProvider.GetRequiredService<IAchievementService>(),
                scope.ServiceProvider.GetRequiredService<INotificationService>()
            );
        }

        private static CommentController CreateCommentController(IServiceScope scope)
        {
            return new CommentController(
                scope.ServiceProvider.GetRequiredService<IBlogService>()
            );
        }
    }
}