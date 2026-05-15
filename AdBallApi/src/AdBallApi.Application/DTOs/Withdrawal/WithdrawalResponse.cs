using AdBallApi.Domain.Enums;

namespace AdBallApi.Application.DTOs.Withdrawal;

public record WithdrawalResponse(
    long WithdrawalId,
    decimal Amount,
    decimal TaxAmount,
    decimal NetAmount,
    WithdrawalStatus Status,
    DateTime RequestedAt
);
