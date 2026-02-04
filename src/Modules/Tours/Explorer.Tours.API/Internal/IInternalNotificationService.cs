
namespace Explorer.Tours.API.Internal
{
    public interface IInternalNotificationService
    {
        void CreateTourPurchaseNotification(long recipientId, long tourId, string tourName);
        void CreateBundlePurchaseNotification(long touristId, long bundleId, string bundleName);
        void CreateTourOnSaleNotification(long recipientId, long tourId, string tourName, decimal discountPercentage);
        void CreateAchievementNotification(long recipientId, string message);
        Task CreateTourRewardAcNotification(long recipientId, int totalAc, int baseReward, int fastCompletionBonus, int streakBonus, string tourName);
    }
}