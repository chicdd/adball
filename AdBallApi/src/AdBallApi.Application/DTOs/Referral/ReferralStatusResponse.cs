namespace AdBallApi.Application.DTOs.Referral;

public record ReferralStatusResponse(
    string MyCode,
    string MyReferralLink,
    int TotalReferrals,
    int PendingBonus,
    int BonusEarned
);
