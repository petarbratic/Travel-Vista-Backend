namespace Explorer.Stakeholders.API.Dtos;

public class WelcomeBonusDto
{
    public long Id { get; set; }
    public long PersonId { get; set; }
    public int BonusType { get; set; }
    public int Value { get; set; }
    public bool IsUsed { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime ExpiresAt { get; set; }
    public DateTime? UsedAt { get; set; }
}
