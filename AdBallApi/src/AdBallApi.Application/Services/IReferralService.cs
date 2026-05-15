using AdBallApi.Application.DTOs.Referral;

namespace AdBallApi.Application.Services;

public interface IReferralService
{
    Task<bool> ApplyReferralCodeAsync(long refereeId, string referralCode, string ipAddress);
    Task<ReferralStatusResponse> GetMyReferralsAsync(long userId);
    Task ProcessReferralBonusIfEligibleAsync(long refereeId);
}
