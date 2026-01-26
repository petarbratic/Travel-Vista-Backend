using AutoMapper;
using Explorer.Tours.API.Dtos;
using Explorer.Tours.API.Internal;
using Explorer.Tours.API.Public;
using Explorer.Tours.Core.Domain;
using Explorer.Tours.Core.Domain.RepositoryInterfaces;

namespace Explorer.Tours.Core.UseCases;

public class NotificationService : INotificationService, IInternalNotificationService
{
    private readonly INotificationRepository _notificationRepository;
    private readonly ITourProblemRepository _tourProblemRepository;
    private readonly ITourRepository _tourRepository;
    private readonly IMapper _mapper;
    private readonly INotificationPublisher _publisher;

    public NotificationService(
        INotificationRepository notificationRepository,
        ITourProblemRepository tourProblemRepository,
        ITourRepository tourRepository,
        IMapper mapper,
        INotificationPublisher publisher)
    {
        _notificationRepository = notificationRepository;
        _tourProblemRepository = tourProblemRepository;
        _tourRepository = tourRepository;
        _mapper = mapper;
        _publisher = publisher;
    }

    public void CreateNewMessageNotification(long recipientId, long problemId, string senderType)
    {
        var problem = _tourProblemRepository.GetById(problemId);
        if (problem == null) return;

        var tour = _tourRepository.GetById(problem.TourId);
        if (tour == null) return;

        var message = senderType == "Tourist"
            ? $"Tourist sent a new message on problem: {TruncateDescription(problem.Description)}"
            : $"Tour author responded to your problem on tour: {tour.Name}";

        var notification = new Notification(
            recipientId: recipientId,
            type: NotificationType.NewMessage,
            relatedEntityId: problemId,
            message: message
        );

        _notificationRepository.Create(notification);
        var notificationDto = _mapper.Map<NotificationDto>(notification);
        _ = _publisher.PublishAsync(notificationDto);
    }

    public void CreateProblemResolvedNotification(long recipientId, long problemId)
    {
        var problem = _tourProblemRepository.GetById(problemId);
        if (problem == null) return;

        var tour = _tourRepository.GetById(problem.TourId);
        if (tour == null) return;

        var notification = new Notification(
            recipientId: recipientId,
            type: NotificationType.ProblemResolved,
            relatedEntityId: problemId,
            message: $"Tourist marked problem as RESOLVED on tour: {tour.Name}"
        );

        _notificationRepository.Create(notification);
        var notificationDto = _mapper.Map<NotificationDto>(notification);
        _ = _publisher.PublishAsync(notificationDto);
    }

    public void CreateProblemUnresolvedNotification(long recipientId, long problemId)
    {
        var problem = _tourProblemRepository.GetById(problemId);
        if (problem == null) return;

        var tour = _tourRepository.GetById(problem.TourId);
        if (tour == null) return;

        var notification = new Notification(
            recipientId: recipientId,
            type: NotificationType.ProblemUnresolved,
            relatedEntityId: problemId,
            message: $"Tourist marked problem as UNRESOLVED on tour: {tour.Name}. Immediate attention required!"
        );

        _notificationRepository.Create(notification);
        var notificationDto = _mapper.Map<NotificationDto>(notification);
        _ = _publisher.PublishAsync(notificationDto);
    }

    public List<NotificationDto> GetMyNotifications(long userId)
    {
        var notifications = _notificationRepository.GetByRecipientId(userId);
        return _mapper.Map<List<NotificationDto>>(notifications);
    }

    public UnreadCountDto GetUnreadCount(long userId)
    {
        var count = _notificationRepository.GetUnreadCountByRecipientId(userId);
        return new UnreadCountDto { Count = count };
    }

    public NotificationDto MarkAsRead(long notificationId, long userId)
    {
        var notification = _notificationRepository.GetById(notificationId);

        if (notification == null)
            throw new KeyNotFoundException($"Notification with id {notificationId} not found.");

        if (notification.RecipientId != userId)
            throw new UnauthorizedAccessException("You don't have permission to access this notification.");

        notification.MarkAsRead();
        var result = _notificationRepository.Update(notification);

        return _mapper.Map<NotificationDto>(result);
    }

    public MarkAllReadResultDto MarkAllAsRead(long userId)
    {
        var updatedCount = _notificationRepository.MarkAllAsReadByRecipientId(userId);
        return new MarkAllReadResultDto { UpdatedCount = updatedCount };
    }

    private string TruncateDescription(string description)
    {
        if (description.Length <= 50)
            return description;

        return description.Substring(0, 50) + "...";
    }

    public void CreateWalletTopUpNotification(long recipientId, int amountAc)
    {
        var notification = new Notification(
            recipientId: recipientId,
            type: NotificationType.WalletTopUp,
            relatedEntityId: recipientId, 
            message: $"Administrator added {amountAc} AC to your wallet."
        );

        _notificationRepository.Create(notification);
        var notificationDto = _mapper.Map<NotificationDto>(notification);
        _ = _publisher.PublishAsync(notificationDto);
    }

    public void CreateTourPurchaseNotification(long recipientId, long tourId, string tourName)
    {
        var notification = new Notification(
            recipientId: recipientId,
            type: NotificationType.TourPurchased,
            relatedEntityId: tourId,
            message: $"You have successfully purchased tour: {tourName}"
        );

        _notificationRepository.Create(notification);
        var notificationDto = _mapper.Map<NotificationDto>(notification);
        _ = _publisher.PublishAsync(notificationDto);
    }
    public void CreateBundlePurchaseNotification(long touristId, long bundleId, string bundleName)
    {
        var notification = new Notification(
            recipientId: touristId,
            type: NotificationType.BundlePurchase,
            relatedEntityId: bundleId,
            message: $"You purchased bundle '{bundleName}' successfully! Check your tours."
        );

        _notificationRepository.Create(notification);
        var notificationDto = _mapper.Map<NotificationDto>(notification);
        _ = _publisher.PublishAsync(notificationDto);
    }

    public void CreateTourOnSaleNotification(long recipientId, long tourId, string tourName, decimal discountPercentage)
    {
        var notification = new Notification(
            recipientId: recipientId,
            type: NotificationType.TourOnSale,
            relatedEntityId: tourId, 
            message: $"Tour '{tourName}' is now on sale: {(double)discountPercentage:0.#}% off!"
        );

        _notificationRepository.Create(notification);
        var notificationDto = _mapper.Map<NotificationDto>(notification);
        _ = _publisher.PublishAsync(notificationDto);
    }

    public void CreateTourPurchaseAchievementNotification(long recipientId, string message)
    {
        var notification = new Notification(
            recipientId: recipientId,
            type: NotificationType.TourPurchaseAchievement,
            relatedEntityId: 1, // not the best practice, but ...
            message: message
        );

        _notificationRepository.Create(notification);
        var notificationDto = _mapper.Map<NotificationDto>(notification);
        _ = _publisher.PublishAsync(notificationDto);
    }

    public void CreateTourCompletedAchievementNotification(long touristId, string message)
    {
        var notification = new Notification(
            recipientId: touristId,
            type: NotificationType.TourCompleteAchievement,
            relatedEntityId: 1, // not the best practice, but ...
            message: message
        );

        _notificationRepository.Create(notification);
        var notificationDto = _mapper.Map<NotificationDto>(notification);
        _ = _publisher.PublishAsync(notificationDto);
    }
    public void CreateClubJoinedAchievementNotification(long touristId, string message)
    {
        var notification = new Notification(
            recipientId: touristId,
            type: NotificationType.ClubJoinAchievement,
            relatedEntityId: 1, // not the best practice, but ...
            message: message
        );

        _notificationRepository.Create(notification);
        var notificationDto = _mapper.Map<NotificationDto>(notification);
        _ = _publisher.PublishAsync(notificationDto);
    }

    public void CreateTourReviewAchievementNotification(long touristId, string message)
    {
        var notification = new Notification(
            recipientId: touristId,
            type: NotificationType.TourReviewAchievement,
            relatedEntityId: 1, // not the best practice, but ...
            message: message
        );

        _notificationRepository.Create(notification);
        var notificationDto = _mapper.Map<NotificationDto>(notification);
        _ = _publisher.PublishAsync(notificationDto);
    }
    public void CreateProfilePicutreAchievementNotification(long touristId, string message)
    {
        var notification = new Notification(
            recipientId: touristId,
            type: NotificationType.ProfilePictureAchievement,
            relatedEntityId: 1, // not the best practice, but ...
            message: message
        );

        _notificationRepository.Create(notification);
        var notificationDto = _mapper.Map<NotificationDto>(notification);
        _ = _publisher.PublishAsync(notificationDto);
    }
    public void CreateAppReviewAchievementNotification(long touristId, string message)
    {
        var notification = new Notification(
            recipientId: touristId,
            type: NotificationType.AppReviewAchievement,
            relatedEntityId: 1, // not the best practice, but ...
            message: message
        );

        _notificationRepository.Create(notification);
        var notificationDto = _mapper.Map<NotificationDto>(notification);
        _ = _publisher.PublishAsync(notificationDto);
    }
    public void CreateBlogCreatedAchievementNotification(long touristId, string message)
    {
        var notification = new Notification(
            recipientId: touristId,
            type: NotificationType.BlogCreatedAchievement,
            relatedEntityId: 1, // not the best practice, but ...
            message: message
        );

        _notificationRepository.Create(notification);
        var notificationDto = _mapper.Map<NotificationDto>(notification);
        _ = _publisher.PublishAsync(notificationDto);
    }

}