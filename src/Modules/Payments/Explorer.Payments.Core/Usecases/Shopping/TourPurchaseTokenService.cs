using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Explorer.Payments.API.Dtos;
using Explorer.Payments.API.Internal;
using Explorer.Stakeholders.API.Internal;
using Explorer.Payments.API.Public.Shopping;
using Explorer.Payments.Core.Domain;
using Explorer.Payments.Core.Domain.RepositoryInterfaces;
using Explorer.Tours.API.Internal;

namespace Explorer.Payments.Core.UseCases.Shopping
{
    public class TourPurchaseTokenService : ITourPurchaseTokenService, IInternalTokenService
    {
        private readonly IShoppingCartRepository _cartRepository;
        private readonly ITourPurchaseTokenRepository _tokenRepository;
        private readonly ITourPurchaseRecordRepository _recordRepository;
        private readonly IInternalWalletService _walletService;
        private readonly IInternalNotificationService _notificationService;
        private readonly IInternalTourService _tourService;
        private readonly IBundlePurchaseRecordRepository _bundlePurchaseRecordRepository;
        private readonly IInternalBundleService _bundleService;
        private readonly IInternalXpEventService _internalXpEventService;
        private readonly IInternalWelcomeBonusService _welcomeBonusService;
        private readonly IInternalAchievementService _achievementService;
        private readonly IInternalTouristRankService _touristRankService;
        private readonly IMapper _mapper;

        public TourPurchaseTokenService(
            IShoppingCartRepository cartRepository,
            ITourPurchaseTokenRepository tokenRepository,
            ITourPurchaseRecordRepository recordRepository,
            IBundlePurchaseRecordRepository bundlePurchaseRecordRepository,
            IInternalWalletService walletService,
            IInternalNotificationService notificationService,
            IInternalTourService tourService,
            IInternalBundleService bundleService,
            IInternalXpEventService xpEventService,
            IInternalWelcomeBonusService welcomeBonusService,
            IInternalAchievementService achievementService,
            IInternalTouristRankService touristRankService,
            IMapper mapper)
        {
            _cartRepository = cartRepository;
            _tokenRepository = tokenRepository;
            _recordRepository = recordRepository;
            _bundlePurchaseRecordRepository = bundlePurchaseRecordRepository;
            _walletService = walletService;
            _notificationService = notificationService;
            _tourService = tourService;
            _bundleService = bundleService;
            _internalXpEventService = xpEventService;
            _welcomeBonusService = welcomeBonusService;
            _achievementService = achievementService;
            _touristRankService = touristRankService;
            _mapper = mapper;
        }

        public CheckoutResultDto Checkout(long touristId)
        {
            try
            {
                var cart = _cartRepository.GetActiveForTourist(touristId);
                if (cart == null || (cart.Items.Count == 0 && cart.BundleItems.Count == 0))
                {
                    return new CheckoutResultDto
                    {
                        Success = false,
                        Message = "Your cart is empty.",
                        Tokens = new List<TourPurchaseTokenDto>(),
                        PurchaseRecords = new List<TourPurchaseRecordDto>(),
                        BundlePurchaseRecords = new List<BundlePurchaseRecordDto>()
                    };
                }

                // Check rank discount
                var rankDiscountPercent = _touristRankService.GetRankDiscountPercentage(touristId);

                // Check welcome bonus discount
                var discountBonus = _welcomeBonusService.GetActiveDiscountBonus(touristId);
                decimal welcomeBonusPercent = discountBonus?.Value ?? 0;

                // Apply the higher discount
                decimal appliedDiscountPercent = Math.Max(rankDiscountPercent, welcomeBonusPercent);
                string discountSource = appliedDiscountPercent == rankDiscountPercent && appliedDiscountPercent > 0
                    ? "Rank Discount"
                    : appliedDiscountPercent > 0 ? "Welcome Bonus" : "None";

                decimal finalPrice = cart.TotalPrice;
                decimal discountAmount = 0;

                if (appliedDiscountPercent > 0)
                {
                    discountAmount = cart.TotalPrice * (appliedDiscountPercent / 100m);
                    finalPrice = cart.TotalPrice - discountAmount;
                }

                var wallet = _walletService.GetWallet(touristId);

                if (wallet.BalanceAc < finalPrice)
                {
                    return new CheckoutResultDto
                    {
                        Success = false,
                        Message = $"Insufficient Adventure Coins. You need {(finalPrice - wallet.BalanceAc):F2} more AC.",
                        Tokens = new List<TourPurchaseTokenDto>(),
                        PurchaseRecords = new List<TourPurchaseRecordDto>(),
                        BundlePurchaseRecords = new List<BundlePurchaseRecordDto>()
                    };
                }

                //_walletService.DeductAc(touristId, finalPrice);
                _walletService.Debit(
                    touristId,
                    (int)finalPrice,
                    WalletTxTypes.CheckoutPurchase,
                    $"Checkout purchase (-{(int)finalPrice} AC)",
                    "Checkout",
                    cart.Id
                );

                // Mark welcome bonus as used ONLY if it was the applied discount
                if (appliedDiscountPercent > 0 && appliedDiscountPercent == welcomeBonusPercent && discountBonus != null)
                {
                    _welcomeBonusService.MarkBonusAsUsed(touristId);
                }

                var tokenDtos = new List<TourPurchaseTokenDto>();
                var recordDtos = new List<TourPurchaseRecordDto>();
                var bundleRecordDtos = new List<BundlePurchaseRecordDto>();

                // Process individual tours
                foreach (var item in cart.Items)
                {
                    var token = new TourPurchaseToken(touristId, item.TourId);
                    var createdToken = _tokenRepository.Create(token);

                    tokenDtos.Add(new TourPurchaseTokenDto
                    {
                        Id = createdToken.Id,
                        TouristId = createdToken.TouristId,
                        TourId = createdToken.TourId,
                        Token = createdToken.Token,
                        CreatedAt = createdToken.CreatedAt
                    });

                    var record = new TourPurchaseRecord(touristId, item.TourId, item.Price);
                    var createdRecord = _recordRepository.Create(record);

                    _internalXpEventService.BuyTourXp(touristId, item.TourId, 20);

                    string message = _achievementService.BoughtTours(touristId);

                    if (!String.Equals(message, ""))
                        _notificationService.CreateAchievementNotification(touristId, message);

                    Console.WriteLine($"    Record created: ID={createdRecord.Id}");

                    recordDtos.Add(new TourPurchaseRecordDto
                    {
                        Id = createdRecord.Id,
                        TouristId = createdRecord.TouristId,
                        TourId = createdRecord.TourId,
                        PriceAc = createdRecord.PriceAc,
                        PurchasedAt = createdRecord.PurchasedAt
                    });

                    try
                    {
                        var tour = _tourService.GetById(item.TourId);
                        if (tour != null)
                        {
                            _notificationService.CreateTourPurchaseNotification(touristId, item.TourId, tour.Name);
                        }
                    }
                    catch
                    {
                        // Notification failed (non-critical)
                    }
                }

                // Process bundle items
                foreach (var bundleItem in cart.BundleItems)
                {
                    var bundle = _bundleService.GetById(bundleItem.BundleId);
                    if (bundle == null)
                        continue;

                    var bundleRecord = new BundlePurchaseRecord(touristId, bundleItem.BundleId, bundleItem.Price);
                    var createdBundleRecord = _bundlePurchaseRecordRepository.Create(bundleRecord);

                    bundleRecordDtos.Add(new BundlePurchaseRecordDto
                    {
                        Id = createdBundleRecord.Id,
                        TouristId = createdBundleRecord.TouristId,
                        BundleId = createdBundleRecord.BundleId,
                        PriceAc = createdBundleRecord.PriceAc,
                        PurchasedAt = createdBundleRecord.PurchasedAt
                    });

                    foreach (var tourId in bundle.TourIds)
                    {
                        var token = new TourPurchaseToken(touristId, tourId);
                        var createdToken = _tokenRepository.Create(token);

                        tokenDtos.Add(new TourPurchaseTokenDto
                        {
                            Id = createdToken.Id,
                            TouristId = createdToken.TouristId,
                            TourId = createdToken.TourId,
                            Token = createdToken.Token,
                            CreatedAt = createdToken.CreatedAt
                        });
                    }

                    try
                    {
                        _notificationService.CreateBundlePurchaseNotification(touristId, bundleItem.BundleId, bundleItem.BundleName);
                    }
                    catch
                    {
                        // Notification failed (non-critical)
                    }
                }

                cart.Clear();
                _cartRepository.Update(cart);

                var result = new CheckoutResultDto
                {
                    Success = true,
                    Message = $"Successfully purchased {recordDtos.Count} tour(s) and {bundleRecordDtos.Count} bundle(s)!" +
                              (appliedDiscountPercent > 0 ? $" ({appliedDiscountPercent}% {discountSource} applied)" : ""),
                    Tokens = tokenDtos,
                    PurchaseRecords = recordDtos,
                    BundlePurchaseRecords = bundleRecordDtos
                };

                return result;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public List<TourPurchaseTokenDto> GetTokens(long touristId)
        {
            var tokens = _tokenRepository.GetByTouristId(touristId);
            return _mapper.Map<List<TourPurchaseTokenDto>>(tokens);
        }
    }
}