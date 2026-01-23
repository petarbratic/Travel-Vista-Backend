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
        // Za profilnu sliku, koristi touristId kao sourceEntityId (jer je samo jednom akcija)
        TryAwardFirstTimeOnce(
            touristId,
            XpEventType.FirstProfilePictureSet,
            AchievementCode.FirstProfilePictureSet,
            ProfilePictureXp
        );
    }

    public void TryAwardFirstAppReview(long touristId, long appRatingId)
    {
        // Za app review, koristi touristId kao sourceEntityId
        TryAwardFirstTimeOnce(
            touristId,
            XpEventType.FirstAppReview,
            AchievementCode.FirstAppReview,
            AppReviewXp
        );
    }

    public void TryAwardFirstClubJoin(long touristId, long clubId)
    {
        // Za klub, koristi touristId kao sourceEntityId (ne clubId)
        TryAwardFirstTimeOnce(
            touristId,
            XpEventType.FirstClubJoined,
            AchievementCode.FirstClubJoined,
            ClubJoinXp
        );
    }

    public void TryAwardFirstBlogCreation(long touristId, long blogId)
    {
        // Za blog, koristi touristId kao sourceEntityId (ne blogId)
        TryAwardFirstTimeOnce(
            touristId,
            XpEventType.FirstBlogCreated,
            AchievementCode.FirstBlogCreated,
            BlogCreationXp
        );
    }

    public void TryAwardFirstBlogCreationByUserId(long userId, long blogId)
    {
        var person = _personRepository.GetByUserId(userId);
        if (person == null) return;

        var tourist = _touristRepository.GetByPersonId(person.Id);
        if (tourist == null) return;

        TryAwardFirstBlogCreation(tourist.Id, blogId);
    }

    // NOVA metoda - proverava samo da li type postoji za turista
    private void TryAwardFirstTimeOnce(
        long touristId,
        XpEventType eventType,
        AchievementCode achievementCode,
        int xpAmount)
    {
        // Proveri da li već ima BILO KOJI event ovog tipa
        int existingCount = _xpEventRepository.CountByType(touristId, eventType);

        if (existingCount > 0)
        {
            // Već je dobio XP za ovu vrstu akcije
            return;
        }

        // Prvi put - kreiraj event sa touristId kao sourceEntityId
        var xpEvent = new XpEvent(touristId, eventType, xpAmount, touristId);
        _xpEventRepository.Create(xpEvent);

        // Dodaj XP
        var tourist = _touristRepository.Get(touristId);
        if (tourist != null)
        {
            tourist.IncreaseXP(xpAmount);
            _touristRepository.Update(tourist);
        }

        // Dodaj achievement
        if (!_achievementRepository.Has(touristId, achievementCode))
        {
            var achievement = new Achievement(touristId, achievementCode);
            _achievementRepository.Create(achievement);
        }
    }
}