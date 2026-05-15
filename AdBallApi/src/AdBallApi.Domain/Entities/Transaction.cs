using AdBallApi.Domain.Enums;

namespace AdBallApi.Domain.Entities;

public class Transaction
{
    public long TxId { get; set; }
    public long UserId { get; set; }
    public TransactionType Type { get; set; }
    public decimal Amount { get; set; }
    public decimal BalanceAfter { get; set; }
    public string? RefId { get; set; }
    public string? Note { get; set; }
    public DateTime CreatedAt { get; set; }
}
