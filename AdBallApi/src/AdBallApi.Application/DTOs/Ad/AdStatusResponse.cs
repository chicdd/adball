namespace AdBallApi.Application.DTOs.Ad;

public record AdStatusResponse(
    int TodayCount,
    int DailyLimit,
    bool CanWatch,
    int CooldownSeconds
);
