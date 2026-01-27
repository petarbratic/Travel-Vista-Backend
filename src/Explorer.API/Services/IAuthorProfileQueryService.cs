using Explorer.API.Dtos.Author;
using Explorer.Payments.Core.Domain.RepositoryInterfaces;
using Explorer.Stakeholders.Core.Domain.RepositoryInterfaces;
using Explorer.Tours.Core.Domain.RepositoryInterfaces;
using Explorer.API.Dtos.Author;

namespace Explorer.API.Services
{
    public interface IAuthorProfileQueryService
    {
        AuthorProfileStatsDto GetMyStats(long currentUserId);
        AuthorProfileStatsDto GetAuthorStats(long authorId);
        List<AuthorTopListItemDto> GetTopAuthors(string sort, int take);
    }
}
