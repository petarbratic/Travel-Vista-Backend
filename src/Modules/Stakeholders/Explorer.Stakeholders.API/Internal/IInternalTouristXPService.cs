namespace Explorer.Stakeholders.API.Internal;

public interface IInternalTouristXPService
{
    void AddExperience(long touristId, int xp);
}