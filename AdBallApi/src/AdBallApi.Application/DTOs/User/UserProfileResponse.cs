namespace AdBallApi.Application.DTOs.User;

public record UserProfileResponse(
    long UserId,
    string ReferralCode,
    string ReferralLink,
    int CurrentRoundTickets,
    decimal WinBalance,
    DateTime CreatedAt
);
