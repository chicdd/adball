using AdBallApi.Domain.Enums;

namespace AdBallApi.Application.DTOs.Draw;

public record DrawResultResponse(
    long RoundId,
    DateOnly WeekStart,
    List<WinnerItem> Winners,
    int TotalTickets,
    string BlockHash,
    long BlockHeight,
    DateTime DrawnAt
);

public record WinnerItem(
    int Rank,
    string MaskedPhone,
    decimal PrizeAmount,
    WinnerStatus Status
);
