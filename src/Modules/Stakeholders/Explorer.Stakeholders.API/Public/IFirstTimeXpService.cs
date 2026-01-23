namespace Explorer.Stakeholders.API.Public;

public interface IFirstTimeXpService
{
    /// Awards XP and achievement for first-time profile picture set
    void TryAwardFirstProfilePicture(long touristId, long personId);

    /// Awards XP and achievement for first app review
    void TryAwardFirstAppReview(long touristId, long appRatingId);

    /// Awards XP and achievement for first club join
    void TryAwardFirstClubJoin(long touristId, long clubId);

    /// Awards XP and achievement for first blog creation
    void TryAwardFirstBlogCreation(long touristId, long blogId);

    /// Helper metoda koja prima userId i sama resolve-uje touristId
    void TryAwardFirstBlogCreationByUserId(long userId, long blogId);
}