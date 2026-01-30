using Explorer.Tours.API.Dtos;

namespace Explorer.Tours.API.Public.Tourist
{
    public interface IGroupTourSessionService
    {
        GroupTourSessionDto GetSessionById(long sessionId);
        List<GroupTourSessionDto >GetSessionsByClubId(long clubId);
        List<GroupTourSessionDto> GetActiveSessionsByClubId(long clubId);
        List<GroupTourSessionDto> GetHighlightedSessionsByClubId(long clubId);
        List<GroupTourSessionDto> GetSessionsForHighlightMarking(long clubId);
        List<GroupTourSessionParticipantDto> GetOtherGroupParticipantsByTouristId(long touristId);
        GroupTourSessionDto GetGroupSessionByTourExecutionId(long tourExecutionId);
        GroupTourSessionDto CreateGroupTourSession(CreateGroupTourSessionDto createDto, long starterId);
        GroupTourSessionDto LeaveGroupTourSession(long sessionId, long touristId);
        GroupTourSessionDto JoinGroupTourSession(long sessionId, long touristId);
        GroupTourSessionDto HighlightGroupTourSession(long sessionId);
        GroupTourSessionDto RefuseHighlightGroupTourSession(long sessionId);
    }
}