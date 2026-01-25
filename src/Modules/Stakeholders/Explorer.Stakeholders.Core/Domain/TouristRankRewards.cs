using Explorer.BuildingBlocks.Core.Domain;

namespace Explorer.Stakeholders.Core.Domain;

public class TouristRankRewards : Entity
{
    public long TouristId { get; private set; }
    public bool GoldRewardClaimed { get; private set; }
    public bool PlatinumRewardClaimed { get; private set; }
    public bool DiamondRewardClaimed { get; private set; }
    public bool VistaRewardClaimed { get; private set; }

    public TouristRankRewards(long touristId)
    {
        if (touristId == 0) throw new ArgumentException("Invalid TouristId");

        TouristId = touristId;
        GoldRewardClaimed = false;
        PlatinumRewardClaimed = false;
        DiamondRewardClaimed = false;
        VistaRewardClaimed = false;
    }

    public void ClaimGoldReward()
    {
        if (GoldRewardClaimed)
            throw new InvalidOperationException("Gold reward already claimed.");
        GoldRewardClaimed = true;
    }

    public void ClaimPlatinumReward()
    {
        if (PlatinumRewardClaimed)
            throw new InvalidOperationException("Platinum reward already claimed.");
        PlatinumRewardClaimed = true;
    }

    public void ClaimDiamondReward()
    {
        if (DiamondRewardClaimed)
            throw new InvalidOperationException("Diamond reward already claimed.");
        DiamondRewardClaimed = true;
    }

    public void ClaimVistaReward()
    {
        if (VistaRewardClaimed)
            throw new InvalidOperationException("Vista reward already claimed.");
        VistaRewardClaimed = true;
    }
}