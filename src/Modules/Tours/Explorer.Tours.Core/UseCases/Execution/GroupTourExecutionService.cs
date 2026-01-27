using Explorer.Tours.API.Dtos;
using Explorer.Tours.API.Public.Execution;
using Explorer.Tours.API.Public.Tourist;
using Explorer.Tours.Core.Domain.RepositoryInterfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime;
using System.Text;
using System.Threading.Tasks;

namespace Explorer.Tours.Core.UseCases.Execution
{
    public class GroupTourExecutionService : IGroupTourExecutionService
    {

        private readonly ITourExecutionService _tourExecutionService;
        private readonly IPositionService _positionService;

        public GroupTourExecutionService(
            ITourExecutionService tourExecutionService,
            IPositionService positionService)
        {
            _tourExecutionService = tourExecutionService;
            _positionService = positionService;
        }

        public long StartGroupExecution(long tourId, long touristId, long sessionId)
        {
            var position = _positionService.GetForTourist(touristId);
            if (position == null)
            {
                position = new PositionDto
                {
                    TouristId = touristId,
                    Latitude = 0,
                    Longitude = 0
                };
            }
            var execution = _tourExecutionService.StartTour(
                new TourExecutionCreateDto
                {
                    TourId = tourId,
                    StartLatitude = position.Latitude,
                    StartLongitude = position.Longitude,
                    GroupSessionId = sessionId,
                },
                touristId
            );

            return execution.Id;
        }

        public long LeaveGroupExecution(long touristId)
        {
            var execution = _tourExecutionService.AbandonTour(touristId);
            if (execution == null) 
                throw new Exception("Failed to abandon tour execution.");

            return execution.Id;
        }

        public bool HasActiveExecution(long touristId)
        {
            if (_tourExecutionService.GetActiveTourExecution(touristId) != null)
                return true;
            return false;
        }
    }
}
