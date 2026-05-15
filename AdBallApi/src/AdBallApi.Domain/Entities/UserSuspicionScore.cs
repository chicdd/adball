namespace AdBallApi.Domain.Entities;

public class UserSuspicionScore
{
    public long ScoreId { get; set; }
    public long UserId { get; set; }
    public int Score { get; set; }
    public string? Reasons { get; set; }
    public DateTime LastCalculatedAt { get; set; }
}
