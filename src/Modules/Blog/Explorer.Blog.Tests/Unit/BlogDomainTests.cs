using Explorer.Blog.Core.Domain.Blogs;
using Shouldly;
using Xunit;

namespace Explorer.Blog.Tests.Unit
{
    public class BlogDomainTests
    {
        [Fact]
        public void Create_blog_with_empty_title_fails()
        {
            var ex = Should.Throw<ArgumentException>(() =>
                new Core.Domain.Blogs.Blog("", "Description", 1)
            );

            ex.Message.ShouldContain("Title cannot be empty");
        }

        [Fact]
        public void AddImage_to_published_blog_fails()
        {
            var blog = new Core.Domain.Blogs.Blog("Title", "Description", 1);
            blog.ChangeStatus(1);

            var image = new BlogImage("https://example.com/image.jpg", blog.Id);

            var ex = Should.Throw<InvalidOperationException>(() =>
                blog.AddImage(image)
            );

            ex.Message.ShouldContain("published");
        }

        [Fact]
        public void RemoveImage_from_published_blog_fails()
        {
            var image = new BlogImage("https://example.com/image.jpg", 1);
            var blog = new Core.Domain.Blogs.Blog("Title", "Description", 1, new List<BlogImage> { image });
            blog.ChangeStatus(1);

            var ex = Should.Throw<InvalidOperationException>(() =>
                blog.RemoveImage(image.Id)
            );

            ex.Message.ShouldContain("published");
        }

        [Fact]
        public void AddImage_null_fails()
        {
            var blog = new Core.Domain.Blogs.Blog("Title", "Description", 1);

            var ex = Should.Throw<ArgumentException>(() =>
                blog.AddImage(null)
            );

            ex.Message.ShouldContain("cannot be null");
        }

        [Fact]
        public void RemoveImage_nonexistent_does_nothing()
        {
            var blog = new Core.Domain.Blogs.Blog("Title", "Description", 1);

            blog.RemoveImage(9999);

            blog.Images.ShouldBeEmpty();
        }

        [Fact]
        public void ChangeStatus_to_readonly_status_fails()
        {
            var blog = new Core.Domain.Blogs.Blog("Title", "Description", 1);

            var ex = Should.Throw<ArgumentException>(() =>
                blog.ChangeStatus(3) // ReadOnly
            );

            ex.Message.ShouldContain("0, 1 ili 2");
        }

        [Fact]
        public void ChangeStatus_when_already_readonly_fails()
        {
            var blog = new Core.Domain.Blogs.Blog("Title", "Description", 1);
            blog.ChangeStatus(1);

            // Downvote do ReadOnly
            for (int i = 1; i <= 11; i++)
            {
                blog.Rate(i, VoteType.Downvote, DateTime.UtcNow);
            }

            var ex = Should.Throw<InvalidOperationException>(() =>
                blog.ChangeStatus(0)
            );

            ex.Message.ShouldContain("read-only");
        }

        [Fact]
        public void GetScore_returns_correct_value()
        {
            var blog = new Core.Domain.Blogs.Blog("Title", "Description", 1);
            blog.ChangeStatus(1);

            blog.Rate(1, VoteType.Upvote, DateTime.UtcNow);
            blog.Rate(2, VoteType.Upvote, DateTime.UtcNow);
            blog.Rate(3, VoteType.Downvote, DateTime.UtcNow);

            blog.GetScore().ShouldBe(1); // 2 upvotes - 1 downvote
        }
    }
}