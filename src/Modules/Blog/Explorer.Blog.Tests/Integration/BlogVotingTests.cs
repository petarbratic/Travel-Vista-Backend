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
    public class BlogVotingTests : BaseBlogIntegrationTest
    {
        public BlogVotingTests(BlogTestFactory factory) : base(factory) { }

        private void AttachUser(BlogController controller, int userId)
        {
            var claims = new List<Claim>
            {
                new Claim("id", userId.ToString()),
                new Claim("personId", userId.ToString()),
                new Claim(ClaimTypes.Role, "author")
            };

            var identity = new ClaimsIdentity(claims, "TestAuth");
            var claimsPrincipal = new ClaimsPrincipal(identity);

            var httpContext = new DefaultHttpContext
            {
                User = claimsPrincipal
            };

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };
        }

        [Fact]
        public void Vote_upvote_increases_score()
        {
            using var scope = Factory.Services.CreateScope();
            var controller = CreateController(scope);
            AttachUser(controller, -11);

            // Kreiraj i objavi blog
            var blog = controller.CreateBlog(new BlogDto
            {
                Title = "Test Voting Blog",
                Description = "For voting tests"
            });

            var createdBlog = ((CreatedAtActionResult)blog.Result).Value as BlogDto;

            // Promeni status u Published
            var service = scope.ServiceProvider.GetRequiredService<IBlogService>();
            service.ChangeStatus(createdBlog.Id, -11, 1);

            // Glasaj kao drugi korisnik
            AttachUser(controller, -12);
            var voteDto = new BlogVoteDto { BlogId = createdBlog.Id, IsUpvote = true };

            var result = controller.Vote(createdBlog.Id, voteDto);

            result.Result.ShouldBeOfType<OkObjectResult>();
            var voteState = ((OkObjectResult)result.Result).Value as BlogVoteStateDto;

            voteState.ShouldNotBeNull();
            voteState.Score.ShouldBe(1);
            voteState.UpvoteCount.ShouldBe(1);
            voteState.DownvoteCount.ShouldBe(0);
            voteState.IsUpvote.ShouldBe(true);
        }

        [Fact]
        public void Vote_downvote_decreases_score()
        {
            using var scope = Factory.Services.CreateScope();
            var controller = CreateController(scope);
            AttachUser(controller, -11);

            var blog = controller.CreateBlog(new BlogDto
            {
                Title = "Test Downvote Blog",
                Description = "For downvote tests"
            });

            var createdBlog = ((CreatedAtActionResult)blog.Result).Value as BlogDto;

            var service = scope.ServiceProvider.GetRequiredService<IBlogService>();
            service.ChangeStatus(createdBlog.Id, -11, 1);

            AttachUser(controller, -12);
            var voteDto = new BlogVoteDto { BlogId = createdBlog.Id, IsUpvote = false };

            var result = controller.Vote(createdBlog.Id, voteDto);

            result.Result.ShouldBeOfType<OkObjectResult>();
            var voteState = ((OkObjectResult)result.Result).Value as BlogVoteStateDto;

            voteState.Score.ShouldBe(-1);
            voteState.DownvoteCount.ShouldBe(1);
            voteState.IsUpvote.ShouldBe(false);
        }

        [Fact]
        public void Vote_toggle_removes_vote()
        {
            using var scope = Factory.Services.CreateScope();
            var controller = CreateController(scope);
            AttachUser(controller, -11);

            var blog = controller.CreateBlog(new BlogDto
            {
                Title = "Toggle Vote Blog",
                Description = "Test toggle"
            });

            var createdBlog = ((CreatedAtActionResult)blog.Result).Value as BlogDto;

            var service = scope.ServiceProvider.GetRequiredService<IBlogService>();
            service.ChangeStatus(createdBlog.Id, -11, 1);

            AttachUser(controller, -12);

            // Prvi upvote
            controller.Vote(createdBlog.Id, new BlogVoteDto { BlogId = createdBlog.Id, IsUpvote = true });

            // Drugi upvote (toggle)
            var result = controller.Vote(createdBlog.Id, new BlogVoteDto { BlogId = createdBlog.Id, IsUpvote = true });

            var voteState = ((OkObjectResult)result.Result).Value as BlogVoteStateDto;
            voteState.Score.ShouldBe(0);
            voteState.IsUpvote.ShouldBeNull();
        }

        [Fact]
        public void Vote_change_from_upvote_to_downvote()
        {
            using var scope = Factory.Services.CreateScope();
            var controller = CreateController(scope);
            AttachUser(controller, -11);

            var blog = controller.CreateBlog(new BlogDto
            {
                Title = "Change Vote Blog",
                Description = "Test change"
            });

            var createdBlog = ((CreatedAtActionResult)blog.Result).Value as BlogDto;

            var service = scope.ServiceProvider.GetRequiredService<IBlogService>();
            service.ChangeStatus(createdBlog.Id, -11, 1);

            AttachUser(controller, -12);

            // Upvote
            controller.Vote(createdBlog.Id, new BlogVoteDto { BlogId = createdBlog.Id, IsUpvote = true });

            // Downvote
            var result = controller.Vote(createdBlog.Id, new BlogVoteDto { BlogId = createdBlog.Id, IsUpvote = false });

            var voteState = ((OkObjectResult)result.Result).Value as BlogVoteStateDto;
            voteState.Score.ShouldBe(-1);
            voteState.IsUpvote.ShouldBe(false);
        }

        [Fact]
        public void Vote_fails_on_draft_blog()
        {
            using var scope = Factory.Services.CreateScope();
            var service = scope.ServiceProvider.GetRequiredService<IBlogService>();

            var blog = service.CreateBlog(new BlogDto
            {
                Title = "Draft Blog",
                Description = "Cannot vote",
                AuthorId = -11
            });

            var ex = Should.Throw<InvalidOperationException>(() =>
                service.Vote(blog.Id, -12, true)
            );

            ex.Message.ShouldContain("published");
        }

        [Fact]
        public void GetVoteState_returns_correct_state()
        {
            using var scope = Factory.Services.CreateScope();
            var controller = CreateController(scope);
            AttachUser(controller, -11);

            var blog = controller.CreateBlog(new BlogDto
            {
                Title = "Vote State Blog",
                Description = "Test state"
            });

            var createdBlog = ((CreatedAtActionResult)blog.Result).Value as BlogDto;

            var service = scope.ServiceProvider.GetRequiredService<IBlogService>();
            service.ChangeStatus(createdBlog.Id, -11, 1);

            AttachUser(controller, -12);
            controller.Vote(createdBlog.Id, new BlogVoteDto { BlogId = createdBlog.Id, IsUpvote = true });

            var result = controller.GetVoteState(createdBlog.Id);

            result.Result.ShouldBeOfType<OkObjectResult>();
            var state = ((OkObjectResult)result.Result).Value as BlogVoteStateDto;

            state.IsUpvote.ShouldBe(true);
            state.Score.ShouldBe(1);
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