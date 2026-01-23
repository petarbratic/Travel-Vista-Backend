using Explorer.Stakeholders.API.Dtos;

namespace Explorer.Stakeholders.API.Public
{
    public interface IMeetupService
    {
        List<MeetupDto> GetAll(long requesterId);
        MeetupDto GetById(long id, long requesterId);
        MeetupDto Create(MeetupCreateDto meetup, long creatorId);
        MeetupDto Update(long id, MeetupUpdateDto meetup, long creatorId);
        void Delete(long id, long creatorId);
        List<MeetupDto> GetByTourId(long tourId, long requesterId);
        List<MeetupMapPreviewDto> GetMapLocations(long requesterId);
    }
}