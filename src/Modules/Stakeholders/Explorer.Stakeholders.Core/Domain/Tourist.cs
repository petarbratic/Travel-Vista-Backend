using Explorer.BuildingBlocks.Core.Domain;

namespace Explorer.Stakeholders.Core.Domain;

public class Tourist : Entity
{
    public long PersonId { get; init; }
    public List<long> EquipmentIds { get; set; }
    public int XP { get; private set; }
    public int Level { get; private set; }

    // Rank property (calculated based on Level)
    public TouristRank Rank => CalculateRank();

    public Tourist(long personId)
    {
        if (personId == 0) throw new ArgumentException("Invalid PersonId");
        PersonId = personId;
        EquipmentIds = new List<long>();
        XP = 0;
        Level = 1;
    }

    public Person Person { get; init; }

    public void IncreaseXP(int xp)
    {
        if (xp < 0) throw new ArgumentException("XP to add cannot be negative.");
        XP += xp;
        const int xpPerLevel = 100;
        var newLevel = (XP / xpPerLevel) + 1;
        if (newLevel > Level)
            Level = newLevel;
    }

    // Metoda za izračunavanje ranka na osnovu levela
    private TouristRank CalculateRank()
    {
        return Level switch
        {
            < 2 => TouristRank.Rookie,
            >= 2 and < 5 => TouristRank.Bronze,
            >= 5 and < 10 => TouristRank.Silver,
            >= 10 and < 15 => TouristRank.Gold,
            >= 15 and < 20 => TouristRank.Platinum,
            >= 20 and < 30 => TouristRank.Diamond,
            _ => TouristRank.Vista
        };
    }
}