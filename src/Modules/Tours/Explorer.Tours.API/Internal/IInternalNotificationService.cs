
namespace Explorer.Tours.API.Internal
{
    public interface IInternalNotificationService
    {
        void CreateTourPurchaseNotification(long recipientId, long tourId, string tourName);
    }
}