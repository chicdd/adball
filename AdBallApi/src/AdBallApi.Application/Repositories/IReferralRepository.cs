using AdBallApi.Domain.Entities;

namespace AdBallApi.Application.Repositories;

public interface IReferralRepository
{
    Task<Referral?> GetByRefereeIdAsync(long refereeId);
    Task<List<Referral>> GetByReferrerIdAsync(long referrerId);
    Task<long> CreateAsync(Referral referral);
    Task UpdateBonusGrantedAsync(long referralId, DateTime grantedAt);
    Task<int> GetReferralCountLast24hAsync(long referrerId);
}
