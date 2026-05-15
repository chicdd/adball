using Dapper;
using AdBallApi.Application.Repositories;
using AdBallApi.Domain.Entities;
using AdBallApi.Infrastructure.Data;

namespace AdBallApi.Infrastructure.Repositories;

public class AdViewRepository : IAdViewRepository
{
    private readonly IDbConnectionFactory _db;

    public AdViewRepository(IDbConnectionFactory db) => _db = db;

    public async Task<int> GetTodayCountAsync(long userId)
    {
        using var conn = _db.CreateConnection();
        return await conn.ExecuteScalarAsync<int>("""
            SELECT COUNT(*) FROM ad_views
            WHERE user_id = @UserId AND DATE(viewed_at) = CURDATE()
            """, new { UserId = userId });
    }

    public async Task<bool> ExistsByTransactionIdAsync(string transactionId)
    {
        using var conn = _db.CreateConnection();
        var count = await conn.ExecuteScalarAsync<int>(
            "SELECT COUNT(*) FROM ad_views WHERE transaction_id = @TransactionId",
            new { TransactionId = transactionId });
        return count > 0;
    }

    public async Task<long> AddAsync(AdView adView)
    {
        using var conn = _db.CreateConnection();
        return await conn.ExecuteScalarAsync<long>("""
            INSERT INTO ad_views (user_id, transaction_id, ad_unit_id, ip_address, reward_granted, abuse_score, viewed_at)
            VALUES (@UserId, @TransactionId, @AdUnitId, @IpAddress, @RewardGranted, @AbuseScore, UTC_TIMESTAMP());
            SELECT LAST_INSERT_ID();
            """, adView);
    }
}
