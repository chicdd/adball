namespace AdBallApi.Application.DTOs.Withdrawal;

public record WithdrawalRequest(
    decimal Amount,
    string BankCode,
    string AccountNumber,
    string AccountHolder
);
