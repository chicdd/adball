using Dapper;
using AdBallApi.Application.Repositories;
using AdBallApi.Domain.Entities;
using AdBallApi.Infrastructure.Data;

namespace AdBallApi.Infrastructure.Repositories;

public class ReferralRepository : IReferralRepository
{
    private readonly IDbConnectionFactory _db;

    public ReferralRepository(IDbConnectionFactory db) => _db = db;

    public async Task<Referral?> GetByRefereeIdAsync(long refereeId)
    {
        using var conn = _db.CreateConnection();
        return await conn.QueryFirstOrDefaultAsync<Referral>(
            "SELECT * FROM referrals WHERE referee_id = @RefereeId", new { RefereeId = refereeId });
    }

    public async Task<List<Referral>> GetByReferrerIdAsync(long referrerId)
    {
        using var conn = _db.CreateConnection();
        var result = await conn.QueryAsync<Referral>(
            "SELECT * FROM referrals WHERE referrer_id = @ReferrerId ORDER BY created_at DESC",
            new { ReferrerId = referrerId });
        return result.ToList();
    }

    public async Task<long> CreateAsync(Referral referral)
    {
        using var conn = _db.CreateConnection();
        return await conn.ExecuteScalarAsync<long>("""
            INSERT INTO referrals (referrer_id, referee_id, created_at)
            VALUES (@ReferrerId, @RefereeId, UTC_TIMESTAMP());
            SELECT LAST_INSERT_ID();
            """, referral);
    }

    public async Task UpdateBonusGrantedAsync(long referralId, DateTime grantedAt)
    {
        using var conn = _db.CreateConnection();
        await conn.ExecuteAsync(
            "UPDATE referrals SET bonus_granted_at = @GrantedAt WHERE referral_id = @ReferralId",
            new { ReferralId = referralId, GrantedAt = grantedAt });
    }

    public async Task<int> GetReferralCountLast24hAsync(long referrerId)
    {
        using var conn = _db.CreateConnection();
        return await conn.ExecuteScalarAsync<int>("""
            SELECT COUNT(*) FROM referrals
            WHERE referrer_id = @ReferrerId AND created_at >= DATE_SUB(UTC_TIMESTAMP(), INTERVAL 24 HOUR)
            """, new { ReferrerId = referrerId });
    }
}
