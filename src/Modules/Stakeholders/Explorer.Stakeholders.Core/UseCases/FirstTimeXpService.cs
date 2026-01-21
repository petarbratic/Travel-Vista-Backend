using Explorer.Stakeholders.API.Public;
using Explorer.Stakeholders.Core.Domain;
using Explorer.Stakeholders.Core.Domain.RepositoryInterfaces;

namespace Explorer.Stakeholders.Core.UseCases;

public class FirstTimeXpService : IFirstTimeXpService
{
    private readonly IXpEventRepository _xpEventRepository;
    private readonly IAchievementRepository _achievementRepository;
    private readonly ITouristRepository _touristRepository;
    private readonly IPersonRepository _personRepository;

    private const int ProfilePictureXp = 100;
    private const int AppReviewXp = 200;
    private const int ClubJoinXp = 100;
    private const int BlogCreationXp = 100;

    public FirstTimeXpService(
        IXpEventRepository xpEventRepository,
        IAchievementRepository achievementRepository,
        ITouristRepository touristRepository,
        IPersonRepository personRepository)
    {
        _xpEventRepository = xpEventRepository;
        _achievementRepository = achievementRepository;
        _touristRepository = touristRepository;
        _personRepository = personRepository;
    }

    public void TryAwardFirstProfilePicture(long touristId, long personId)
    {
        TryAwardFirstTime(
            touristId,
            personId,
            XpEventType.FirstProfilePictureSet,
            AchievementCode.FirstProfilePictureSet,
            ProfilePictureXp
        );
    }

    public void TryAwardFirstAppReview(long touristId, long appRatingId)
    {
        TryAwardFirstTime(
            touristId,
            appRatingId,
            XpEventType.FirstAppReview,
            AchievementCode.FirstAppReview,
            AppReviewXp
        );
    }

    public void TryAwardFirstClubJoin(long touristId, long clubId)
    {
        TryAwardFirstTime(
            touristId,
            clubId,
            XpEventType.FirstClubJoined,
            AchievementCode.FirstClubJoined,
            ClubJoinXp
        );
    }

    public void TryAwardFirstBlogCreation(long touristId, long blogId)
    {
        TryAwardFirstTime(
            touristId,
            blogId,
            XpEventType.FirstBlogCreated,
            AchievementCode.FirstBlogCreated,
            BlogCreationXp
        );
    }

    public void TryAwardFirstBlogCreationByUserId(long userId, long blogId)
    {
        var person = _personRepository.GetByUserId(userId);
        if (person == null)
        {
            return;
        }

        // ISPRAVLJENO: Koristi GetByPersonId umesto Get
        var tourist = _touristRepository.GetByPersonId(person.Id);
        if (tourist == null)
        {
            return;
        }

        TryAwardFirstTime(
            tourist.Id,
            blogId,
            XpEventType.FirstBlogCreated,
            AchievementCode.FirstBlogCreated,
            BlogCreationXp
        );
    }

    private void TryAwardFirstTime(
        long touristId,
        long sourceEntityId,
        XpEventType eventType,
        AchievementCode achievementCode,
        int xpAmount)
    {
        if (_xpEventRepository.Exists(touristId, eventType, sourceEntityId))
        {
            return;
        }

        var xpEvent = new XpEvent(touristId, eventType, xpAmount, sourceEntityId);
        _xpEventRepository.Create(xpEvent);

        var tourist = _touristRepository.Get(touristId);
        if (tourist != null)
        {
            tourist.IncreaseXP(xpAmount);
            _touristRepository.Update(tourist);
        }

        if (!_achievementRepository.Has(touristId, achievementCode))
        {
            var achievement = new Achievement(touristId, achievementCode);
            _achievementRepository.Create(achievement);
        }
    }
}