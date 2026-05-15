using AdBallApi.Domain.Enums;

namespace AdBallApi.Application.DTOs.Draw;

public record RoundResponse(
    long RoundId,
    DateOnly WeekStart,
    decimal AdRevenue,
    decimal PrizePool,
    RoundStatus Status,
    DateTime? DrawAt
);
