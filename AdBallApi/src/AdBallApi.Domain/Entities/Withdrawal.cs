using AdBallApi.Domain.Enums;

namespace AdBallApi.Domain.Entities;

public class Withdrawal
{
    public long WithdrawalId { get; set; }
    public long UserId { get; set; }
    public decimal Amount { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal NetAmount { get; set; }
    public string BankCode { get; set; } = "";
    public string AccountNumber { get; set; } = "";
    public string AccountHolder { get; set; } = "";
    public WithdrawalStatus Status { get; set; } = WithdrawalStatus.Pending;
    public DateTime RequestedAt { get; set; }
    public DateTime? ProcessedAt { get; set; }
    public string? RejectReason { get; set; }
}
