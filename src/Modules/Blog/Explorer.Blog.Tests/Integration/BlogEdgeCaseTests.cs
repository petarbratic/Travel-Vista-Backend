using Explorer.API.Controllers.Author_Tourist;
using Explorer.Blog.API.Dtos;
using Explorer.Blog.API.Public;
using Explorer.Blog.Core.Domain.Blogs;
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
    public class BlogEdgeCaseTests : BaseBlogIntegrationTest
    {
        public BlogEdgeCaseTests(BlogTestFactory factory) : base(factory) { }

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
        public void Update_published_blog_cannot_change_title()
        {
            using var scope = Factory.Services.CreateScope();
            var service = scope.ServiceProvider.GetRequiredService<IBlogService>();

            var blog = service.CreateBlog(new BlogDto
            {
                Title = "Original Title",
                Description = "Description",
                AuthorId = -11
            });

            service.ChangeStatus(blog.Id, -11, 1);

            blog.Title = "New Title";
            blog.Description = "New Description";

            var ex = Should.Throw<InvalidOperationException>(() => service.UpdateBlog(blog));
            ex.Message.ShouldContain("Cannot change title");
        }

        [Fact]
        public void Update_published_blog_can_change_description()
        {
            using var scope = Factory.Services.CreateScope();
            var service = scope.ServiceProvider.GetRequiredService<IBlogService>();

            var blog = service.CreateBlog(new BlogDto
            {
                Title = "Title",
                Description = "Original",
                AuthorId = -11
            });

            service.ChangeStatus(blog.Id, -11, 1);

            blog.Description = "Updated Description";

            var updated = service.UpdateBlog(blog);
            updated.Description.ShouldBe("Updated Description");
        }

        [Fact]
        public void Update_archived_blog_fails()
        {
            using var scope = Factory.Services.CreateScope();
            var service = scope.ServiceProvider.GetRequiredService<IBlogService>();

            var blog = service.CreateBlog(new BlogDto
            {
                Title = "Title",
                Description = "Description",
                AuthorId = -11
            });

            service.ChangeStatus(blog.Id, -11, 2); // Archived

            blog.Description = "Try to update";

            var ex = Should.Throw<InvalidOperationException>(() => service.UpdateBlog(blog));
            ex.Message.ShouldContain("archived");
        }

        [Fact]
        public void EditComment_fails_for_different_user()
        {
            using var scope = Factory.Services.CreateScope();
            var service = scope.ServiceProvider.GetRequiredService<IBlogService>();

            var blog = service.CreateBlog(new BlogDto
            {
                Title = "Title",
                Description = "Description",
                AuthorId = -11
            });

            service.ChangeStatus(blog.Id, -11, 1);

            var comment = service.AddComment(blog.Id, -12, "Original");

            var ex = Should.Throw<InvalidOperationException>(() => 
                service.EditComment(blog.Id, comment.Id, -13, "Hacked!")
            );

            ex.Message.ShouldContain("your own");
        }

        [Fact]
        public void DeleteComment_fails_for_different_user()
        {
            using var scope = Factory.Services.CreateScope();
            var service = scope.ServiceProvider.GetRequiredService<IBlogService>();

            var blog = service.CreateBlog(new BlogDto
            {
                Title = "Title",
                Description = "Description",
                AuthorId = -11
            });

            service.ChangeStatus(blog.Id, -11, 1);

            var comment = service.AddComment(blog.Id, -12, "Original");

            var ex = Should.Throw<InvalidOperationException>(() => 
                service.DeleteComment(blog.Id, comment.Id, -13)
            );

            ex.Message.ShouldContain("your own");
        }

        [Fact]
        public void Vote_on_archived_blog_fails()
        {
            using var scope = Factory.Services.CreateScope();
            var service = scope.ServiceProvider.GetRequiredService<IBlogService>();

            var blog = service.CreateBlog(new BlogDto
            {
                Title = "Title",
                Description = "Description",
                AuthorId = -11
            });

            service.ChangeStatus(blog.Id, -11, 1);
            service.ChangeStatus(blog.Id, -11, 2); // Archive

            var ex = Should.Throw<InvalidOperationException>(() => 
                service.Vote(blog.Id, -12, true)
            );

            ex.Message.ShouldContain("archived");
        }

        [Fact]
        public void AddComment_on_readonly_blog_fails()
        {
            using var scope = Factory.Services.CreateScope();
            var service = scope.ServiceProvider.GetRequiredService<IBlogService>();

            // Kreiraj blog i postavi ga u ReadOnly stanje preko downvote-ova
            var blog = service.CreateBlog(new BlogDto
            {
                Title = "ReadOnly Test",
                Description = "Will become readonly",
                AuthorId = -11
            });

            service.ChangeStatus(blog.Id, -11, 1);

            // Dodaj 11 downvote-ova (score = -11)
            for (int i = 1; i <= 11; i++)
            {
                service.Vote(blog.Id, -10 - i, false);
            }

            var ex = Should.Throw<InvalidOperationException>(() =>
                service.AddComment(blog.Id, -12, "Comment on readonly")
            );

            ex.Message.ShouldContain("read-only");
        }

        [Fact]
        public void Vote_on_readonly_blog_fails()
        {
            using var scope = Factory.Services.CreateScope();
            var service = scope.ServiceProvider.GetRequiredService<IBlogService>();

            var blog = service.CreateBlog(new BlogDto
            {
                Title = "ReadOnly Vote Test",
                Description = "Test",
                AuthorId = -11
            });

            service.ChangeStatus(blog.Id, -11, 1);

            // 11 downvotes = ReadOnly
            for (int i = 1; i <= 11; i++)
            {
                service.Vote(blog.Id, -10 - i, false);
            }

            var ex = Should.Throw<InvalidOperationException>(() =>
                service.Vote(blog.Id, -50, true)
            );

            ex.Message.ShouldContain("read-only");
        }

        [Fact]
        public void Blog_becomes_active_with_high_score_and_comments()
        {
            using var scope = Factory.Services.CreateScope();
            var service = scope.ServiceProvider.GetRequiredService<IBlogService>();

            var blog = service.CreateBlog(new BlogDto
            {
                Title = "Active Blog Test",
                Description = "Will become active",
                AuthorId = -11
            });

            service.ChangeStatus(blog.Id, -11, 1);

            // 101 upvotes
            for (int i = 1; i <= 101; i++)
            {
                service.Vote(blog.Id, -100 - i, true);
            }

            // 11 comments
            for (int i = 1; i <= 11; i++)
            {
                service.AddComment(blog.Id, -200 - i, $"Comment {i}");
            }

            var result = service.GetUserVoteState(blog.Id, -11);
            result.BlogStatus.ShouldBe((int)BlogStatus.Active); // Active = 3
        }

        [Fact]
        public void Blog_becomes_famous_with_very_high_score_and_many_comments()
        {
            using var scope = Factory.Services.CreateScope();
            var service = scope.ServiceProvider.GetRequiredService<IBlogService>();

            var blog = service.CreateBlog(new BlogDto
            {
                Title = "Famous Blog Test",
                Description = "Will become famous",
                AuthorId = -11
            });

            service.ChangeStatus(blog.Id, -11, 1);

            // 501 upvotes
            for (int i = 1; i <= 501; i++)
            {
                service.Vote(blog.Id, -1000 - i, true);
            }

            // 31 comments
            for (int i = 1; i <= 31; i++)
            {
                service.AddComment(blog.Id, -2000 - i, $"Comment {i}");
            }

            var result = service.GetUserVoteState(blog.Id, -11);
            result.BlogStatus.ShouldBe((int)BlogStatus.Famous); // Famous = 4
        }

        [Fact]
        public void Vote_on_nonexistent_blog_fails()
        {
            using var scope = Factory.Services.CreateScope();
            var service = scope.ServiceProvider.GetRequiredService<IBlogService>();

            var ex = Should.Throw<KeyNotFoundException>(() =>
                service.Vote(-9999, -12, true)
            );

            ex.Message.ShouldContain("nije pronađen");
        }

        [Fact]
        public void GetUserVoteState_on_nonexistent_blog_fails()
        {
            using var scope = Factory.Services.CreateScope();
            var service = scope.ServiceProvider.GetRequiredService<IBlogService>();

            var ex = Should.Throw<KeyNotFoundException>(() =>
                service.GetUserVoteState(-9999, -12)
            );

            ex.Message.ShouldContain("nije pronađen");
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