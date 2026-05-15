using AdBallApi.Domain.Enums;

namespace AdBallApi.Domain.Entities;

public class Round
{
    public long RoundId { get; set; }
    public DateOnly WeekStart { get; set; }
    public decimal AdRevenue { get; set; }
    public decimal PrizePool { get; set; }
    public RoundStatus Status { get; set; } = RoundStatus.Open;
    public DateTime? DrawAt { get; set; }
    public DateTime CreatedAt { get; set; }
}
