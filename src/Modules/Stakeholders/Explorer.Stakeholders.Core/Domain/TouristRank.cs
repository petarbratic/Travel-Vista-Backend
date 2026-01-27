namespace Explorer.Stakeholders.Core.Domain;

public enum TouristRank
{
    Rookie = 0,    // x < 2
    Bronze = 1,    // 2 <= x < 5
    Silver = 2,    // 5 <= x < 10
    Gold = 3,      // 10 <= x < 15
    Platinum = 4,  // 15 <= x < 20
    Diamond = 5,   // 20 <= x < 30
    Vista = 6      // 30+
}