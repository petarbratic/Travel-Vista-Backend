// src/Modules/Payments/Explorer.Payments.Core/UseCases/Shopping/TourPurchaseTokenService.cs
using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Explorer.Payments.API.Dtos;
using Explorer.Payments.API.Internal;
using Explorer.Payments.API.Public.Shopping;
using Explorer.Payments.Core.Domain;
using Explorer.Payments.Core.Domain.RepositoryInterfaces;
using Explorer.Stakeholders.API.Internal;  // ← DODAJ OVO za IInternalWalletService
using Explorer.Tours.API.Internal;         // ← DODAJ OVO za IInternalNotificationService

namespace Explorer.Payments.Core.UseCases.Shopping
{
    public class TourPurchaseTokenService : ITourPurchaseTokenService, IInternalTokenService
    {
        private readonly IShoppingCartRepository _cartRepository;
        private readonly ITourPurchaseTokenRepository _tokenRepository;
        private readonly ITourPurchaseRecordRepository _recordRepository;
        private readonly IInternalWalletService _walletService;
        private readonly IInternalNotificationService _notificationService;
        private readonly IMapper _mapper;

        public TourPurchaseTokenService(
            IShoppingCartRepository cartRepository,
            ITourPurchaseTokenRepository tokenRepository,
            ITourPurchaseRecordRepository recordRepository,
            IInternalWalletService walletService,
            IInternalNotificationService notificationService,
            IMapper mapper)
        {
            _cartRepository = cartRepository;
            _tokenRepository = tokenRepository;
            _recordRepository = recordRepository;
            _walletService = walletService;
            _notificationService = notificationService;
            _mapper = mapper;
        }

        public CheckoutResultDto Checkout(long touristId)  // ← PROMIJENI povratni tip
        {
            var cart = _cartRepository.GetActiveForTourist(touristId)
                      ?? throw new InvalidOperationException("Cart not found.");

            if (cart.Items.Count == 0)
                throw new InvalidOperationException("Cart is empty.");

            // Provjera da li turista ima dovoljno AC-a
            var wallet = _walletService.GetWallet(touristId);
            if (wallet.BalanceAc < cart.TotalPrice)
            {
                return new CheckoutResultDto
                {
                    Success = false,
                    Message = $"Insufficient balance. You have {wallet.BalanceAc} AC, but need {cart.TotalPrice} AC."
                };
            }

            // Oduzimanje AC-a
            _walletService.DeductAc(touristId, cart.TotalPrice);

            var tokens = new List<TourPurchaseToken>();
            var records = new List<TourPurchaseRecord>();

            foreach (var item in cart.Items)
            {
                var token = new TourPurchaseToken(
                    touristId,
                    item.TourId,
                    Guid.NewGuid().ToString()
                );
                tokens.Add(_tokenRepository.Create(token));

                var record = new TourPurchaseRecord(
                   touristId,
                   item.TourId,
                   item.Price
                );
                records.Add(_recordRepository.Create(record));

                // Slanje notifikacije za svaku kupljenu turu
                _notificationService.CreateTourPurchaseNotification(touristId, item.TourId, item.TourName);
            }

            cart.Clear();
            _cartRepository.Update(cart);

            return new CheckoutResultDto
            {
                Success = true,
                Message = $"Successfully purchased {tokens.Count} tour(s).",
                Tokens = _mapper.Map<List<TourPurchaseTokenDto>>(tokens),
                PurchaseRecords = _mapper.Map<List<TourPurchaseRecordDto>>(records)
            };
        }

        public List<TourPurchaseTokenDto> GetTokens(long touristId)
        {
            var tokens = _tokenRepository.GetByTouristId(touristId);
            return _mapper.Map<List<TourPurchaseTokenDto>>(tokens);
        }
    }
}