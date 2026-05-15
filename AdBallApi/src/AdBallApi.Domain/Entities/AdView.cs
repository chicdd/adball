namespace AdBallApi.Domain.Entities;

public class AdView
{
    public long AdViewId { get; set; }
    public long UserId { get; set; }
    public string TransactionId { get; set; } = "";
    public string? AdUnitId { get; set; }
    public string? IpAddress { get; set; }
    public bool RewardGranted { get; set; }
    public int AbuseScore { get; set; }
    public DateTime ViewedAt { get; set; }
}
