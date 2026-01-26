using AutoMapper;
using Explorer.Tours.API.Dtos;
using Explorer.Tours.API.Public.Execution;
using Explorer.Tours.Core.Domain.RepositoryInterfaces;
using System.Collections.Generic;
using System.Linq;

namespace Explorer.Tours.Core.UseCases.Execution;

public class TourHistoryService : ITourHistoryService
{
    private readonly ITourExecutionRepository _executionRepository;
    private readonly ITourRepository _tourRepository;
    private readonly IMapper _mapper;

    public TourHistoryService(
        ITourExecutionRepository executionRepository,
        ITourRepository tourRepository,
        IMapper mapper)
    {
        _executionRepository = executionRepository;
        _tourRepository = tourRepository;
        _mapper = mapper;
    }

    public TourHistoryOverviewDto GetTourHistory(long touristId)
    {
        // 1. Dobij sve kompletirane ture turiste
        var completedExecutions = _executionRepository.GetCompletedByTouristId(touristId);
        
        var completedTours = new List<TourHistoryDto>();
        double totalDistance = 0;
        double totalTime = 0;

        foreach (var execution in completedExecutions)
        {
            var tour = _tourRepository.GetByIdWithKeyPoints(execution.TourId);
            if (tour == null) continue;

            var durationMinutes = execution.CompletionTime.HasValue
                ? (execution.CompletionTime.Value - execution.StartTime).TotalMinutes
                : 0;

            var firstKeyPoint = tour.KeyPoints?.OrderBy(k => k.Id).FirstOrDefault();

            completedTours.Add(new TourHistoryDto
            {
                ExecutionId = execution.Id,
                TourId = tour.Id,
                TourName = tour.Name,
                CompletedAt = execution.CompletionTime ?? execution.StartTime,
                Location = firstKeyPoint?.Name ?? "Unknown",
                DistanceInKm = tour.DistanceInKm,
                DurationInMinutes = durationMinutes
            });

            totalDistance += tour.DistanceInKm;
            totalTime += durationMinutes;
        }

        // 2. Izračunaj statistiku
        var totalPurchased = _executionRepository.GetTotalPurchasedToursCount(touristId);
        var completionRate = totalPurchased > 0
            ? (completedExecutions.Count / (double)totalPurchased) * 100
            : 0;

        var statistics = new TourStatisticsDto
        {
            TotalCompletedTours = completedExecutions.Count,
            TotalDistanceTraveled = totalDistance,
            TotalTimeSpent = totalTime,
            CompletionRate = completionRate
        };

        // 3. Izračunaj prosek svih turista
        var allCompleted = _executionRepository.GetAllCompleted();
        var touristIds = allCompleted.Select(e => e.TouristId).Distinct().ToList();
        var avgCompletedTours = touristIds.Any() ? allCompleted.Count / (double)touristIds.Count : 0;

        double avgTotalDistance = 0;
        double avgCompletionRate = 0;

        if (touristIds.Any())
        {
            var totalDistances = new List<double>();
            var completionRates = new List<double>();

            foreach (var tid in touristIds)
            {
                var userCompleted = allCompleted.Where(e => e.TouristId == tid).ToList();
                var userDistance = 0.0;

                foreach (var exec in userCompleted)
                {
                    var tour = _tourRepository.GetById(exec.TourId);
                    if (tour != null) userDistance += tour.DistanceInKm;
                }

                totalDistances.Add(userDistance);

                var userPurchased = _executionRepository.GetTotalPurchasedToursCount(tid);
                var userRate = userPurchased > 0 ? (userCompleted.Count / (double)userPurchased) * 100 : 0;
                completionRates.Add(userRate);
            }

            avgTotalDistance = totalDistances.Average();
            avgCompletionRate = completionRates.Average();
        }

        // 4. Poređenje
        var toursPercentageDiff = avgCompletedTours > 0
            ? ((completedExecutions.Count - avgCompletedTours) / avgCompletedTours) * 100
            : 0;

        var distancePercentageDiff = avgTotalDistance > 0
            ? ((totalDistance - avgTotalDistance) / avgTotalDistance) * 100
            : 0;

        var completionRateDiff = avgCompletionRate > 0
            ? completionRate - avgCompletionRate
            : 0;

        var comparison = new TourComparisonDto
        {
            YourCompletedTours = completedExecutions.Count,
            AverageCompletedTours = (int)avgCompletedTours,
            ToursPercentageDifference = toursPercentageDiff,

            YourCompletionRate = completionRate,
            AverageCompletionRate = avgCompletionRate,
            CompletionRateDifference = completionRateDiff,

            YourTotalDistance = totalDistance,
            AverageTotalDistance = avgTotalDistance,
            DistancePercentageDifference = distancePercentageDiff
        };

        return new TourHistoryOverviewDto
        {
            CompletedTours = completedTours,
            Statistics = statistics,
            Comparison = comparison
        };
    }
}
