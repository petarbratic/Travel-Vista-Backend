using Explorer.Stakeholders.API.Internal;
using Explorer.Tours.Core.Domain;
using Explorer.Tours.Core.Domain.RepositoryInterfaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Explorer.Tours.Core.UseCases.Execution;

public class AcRewardsService
{
    private readonly ITourExecutionRepository _executionRepository;
    private readonly ITourRepository _tourRepository;
    private readonly IInternalWalletService _walletService;

    // Konfiguracija nagrada
    private const int BaseRewardMin = 5;
    private const int BaseRewardMax = 10;
    private const int FastCompletionBonus = 5; // Bonus za brzu završnicu
    private const double FastCompletionThresholdHours = 2.0; // Tura mora biti završena za manje od 2 sata
    private const int StreakBonus = 10; // Bonus za streak
    private const int StreakToursRequired = 3; // Broj tura za streak
    private const int StreakDaysWindow = 7; // Vremenski prozor za streak (7 dana)

    public AcRewardsService(
        ITourExecutionRepository executionRepository,
        ITourRepository tourRepository,
        IInternalWalletService walletService)
    {
        _executionRepository = executionRepository;
        _tourRepository = tourRepository;
        _walletService = walletService;
    }

    public AcRewardResult CalculateAndAwardRewards(long touristId, long tourId, DateTime startTime, DateTime completionTime)
    {
        var result = new AcRewardResult();

        // 1. Osnovna nagrada (5-10 AC)
        var random = new Random();
        result.BaseReward = random.Next(BaseRewardMin, BaseRewardMax + 1);
        result.TotalAc = result.BaseReward;

        // 2. Bonus za brzu završnicu
        var duration = completionTime - startTime;
        if (duration.TotalHours <= FastCompletionThresholdHours)
        {
            result.FastCompletionBonus = FastCompletionBonus;
            result.TotalAc += result.FastCompletionBonus;
        }

        // 3. Streak bonus (3 ture u 7 dana)
        if (HasStreak(touristId, completionTime))
        {
            result.StreakBonus = StreakBonus;
            result.TotalAc += result.StreakBonus;
        }

        // Dodaj AC u wallet
        if (result.TotalAc > 0)
        {
            var tour = _tourRepository.GetById(tourId);
            var tourName = tour?.Name ?? "Tour";
            
            var description = BuildRewardDescription(result, tourName);
            
            _walletService.Credit(
                personId: touristId,
                amountAc: result.TotalAc,
                type: 5, // WalletTransactionType.TourRewardAc
                description: description,
                referenceType: "TourCompletion",
                referenceId: tourId
            );
        }

        return result;
    }

    private bool HasStreak(long touristId, DateTime currentCompletionTime)
    {
        // Uzmi sve kompletirane ture turiste (uključujući trenutnu koja je već sačuvana)
        var completedTours = _executionRepository.GetCompletedByTouristId(touristId)
            .Where(te => te.CompletionTime.HasValue)
            .OrderByDescending(te => te.CompletionTime!.Value)
            .ToList();

        if (completedTours.Count < StreakToursRequired)
            return false;

        // Uzmi poslednje N tura (uključujući trenutnu)
        var recentTours = completedTours.Take(StreakToursRequired).ToList();
        
        // Proveri da li su sve poslednje N tura kompletirane unutar 7 dana jedna od druge
        if (recentTours.Count == StreakToursRequired)
        {
            var oldestCompletion = recentTours.Min(te => te.CompletionTime!.Value);
            var newestCompletion = recentTours.Max(te => te.CompletionTime!.Value);
            
            // Streak postoji ako je razlika između najstarije i najnovije ture <= 7 dana
            return (newestCompletion - oldestCompletion).TotalDays <= StreakDaysWindow;
        }

        return false;
    }

    private string BuildRewardDescription(AcRewardResult result, string tourName)
    {
        var parts = new List<string>();
        
        parts.Add($"Osnovna nagrada: +{result.BaseReward} AC");
        
        if (result.FastCompletionBonus > 0)
        {
            parts.Add($"Bonus za brzu završnicu: +{result.FastCompletionBonus} AC");
        }
        
        if (result.StreakBonus > 0)
        {
            parts.Add($"Streak bonus ({StreakToursRequired} ture u {StreakDaysWindow} dana): +{result.StreakBonus} AC");
        }
        
        return $"Nagrada za završenu turu '{tourName}': {string.Join(", ", parts)}";
    }
}

public class AcRewardResult
{
    public int BaseReward { get; set; }
    public int FastCompletionBonus { get; set; }
    public int StreakBonus { get; set; }
    public int TotalAc { get; set; }
}
