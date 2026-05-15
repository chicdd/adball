using AdBallApi.Domain.Enums;

namespace AdBallApi.Domain.Entities;

public class Winner
{
    public long WinnerId { get; set; }
    public long RoundId { get; set; }
    public long UserId { get; set; }
    public int Rank { get; set; }
    public decimal PrizeAmount { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal NetAmount { get; set; }
    public WinnerStatus Status { get; set; } = WinnerStatus.Pending;
    public DateTime CreatedAt { get; set; }
}
