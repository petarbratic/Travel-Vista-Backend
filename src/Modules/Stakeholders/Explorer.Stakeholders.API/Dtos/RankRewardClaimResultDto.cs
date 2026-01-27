namespace Explorer.Stakeholders.API.Dtos;

public class RankRewardClaimResultDto
{
    public bool Success { get; set; }
    public string Message { get; set; }
    public int AcAwarded { get; set; }
    public List<string> ClaimedRanks { get; set; } = new();
}