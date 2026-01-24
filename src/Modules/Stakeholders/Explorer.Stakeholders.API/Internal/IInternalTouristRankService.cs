namespace Explorer.Stakeholders.API.Internal;

public interface IInternalTouristRankService
{
    decimal GetRankDiscountPercentage(long touristId);
}