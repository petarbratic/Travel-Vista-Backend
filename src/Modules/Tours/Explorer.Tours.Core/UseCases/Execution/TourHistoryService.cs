using AutoMapper;
using Explorer.Tours.API.Dtos;
using Explorer.Tours.API.Public.Execution;
using Explorer.Tours.Core.Domain.RepositoryInterfaces;
using Explorer.Stakeholders.API.Internal;
using System.Collections.Generic;
using System.Linq;

namespace Explorer.Tours.Core.UseCases.Execution;

public class TourHistoryService : ITourHistoryService
{
    private readonly ITourExecutionRepository _executionRepository;
    private readonly ITourRepository _tourRepository;
    private readonly IInternalTouristService _touristService;
    private readonly IMapper _mapper;

    public TourHistoryService(
        ITourExecutionRepository executionRepository,
        ITourRepository tourRepository,
        IInternalTouristService touristService,
        IMapper mapper)
    {
        _executionRepository = executionRepository;
        _tourRepository = tourRepository;
        _touristService = touristService;
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

        // 2. Izračunaj unique completed tours (bez duplikata)
        var uniqueCompletedTours = completedExecutions
            .Select(e => e.TourId)
            .Distinct()
            .Count();

        var statistics = new TourStatisticsDto
        {
            TotalCompletedTours = uniqueCompletedTours,
            TotalDistanceTraveled = totalDistance,
            TotalTimeSpent = totalTime
        };

        // 3. Izračunaj prosek svih turista
        var allTouristIds = _touristService.GetAllTouristPersonIds();
        var allCompleted = _executionRepository.GetAllCompleted();

        double avgCompletedTours = 0;
        double avgTotalDistance = 0;

        if (allTouristIds.Any())
        {
            var totalDistances = new List<double>();
            var completedCounts = new List<int>();

            foreach (var tid in allTouristIds)
            {
                var userCompleted = allCompleted.Where(e => e.TouristId == tid).ToList();
                
                // Broji unique tours
                var userUniqueCompleted = userCompleted
                    .Select(e => e.TourId)
                    .Distinct()
                    .Count();
                
                completedCounts.Add(userUniqueCompleted);

                // Saberi distance bez duplikata
                var userDistance = 0.0;
                var processedTourIds = new HashSet<long>();

                foreach (var exec in userCompleted)
                {
                    if (processedTourIds.Add(exec.TourId))
                    {
                        var tour = _tourRepository.GetById(exec.TourId);
                        if (tour != null) userDistance += tour.DistanceInKm;
                    }
                }
                totalDistances.Add(userDistance);
            }

            avgCompletedTours = completedCounts.Average();
            avgTotalDistance = totalDistances.Average();
        }

        // 4. Poređenje
        var roundedAvgTours = Math.Round(avgCompletedTours);

        var toursPercentageDiff = roundedAvgTours > 0
            ? ((uniqueCompletedTours - roundedAvgTours) / roundedAvgTours) * 100
            : 0;

        var distancePercentageDiff = avgTotalDistance > 0
            ? ((totalDistance - avgTotalDistance) / avgTotalDistance) * 100
            : 0;

        var comparison = new TourComparisonDto
        {
            YourCompletedTours = uniqueCompletedTours,
            AverageCompletedTours = (int)roundedAvgTours,
            ToursPercentageDifference = toursPercentageDiff,

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
