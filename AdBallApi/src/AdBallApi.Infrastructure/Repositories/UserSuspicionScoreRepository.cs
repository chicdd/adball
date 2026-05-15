using Dapper;
using AdBallApi.Application.Repositories;
using AdBallApi.Domain.Entities;
using AdBallApi.Infrastructure.Data;

namespace AdBallApi.Infrastructure.Repositories;

public class UserSuspicionScoreRepository : IUserSuspicionScoreRepository
{
    private readonly IDbConnectionFactory _db;

    public UserSuspicionScoreRepository(IDbConnectionFactory db) => _db = db;

    public async Task UpsertAsync(UserSuspicionScore score)
    {
        using var conn = _db.CreateConnection();
        await conn.ExecuteAsync("""
            INSERT INTO user_suspicion_scores (user_id, score, reasons, last_calculated_at)
            VALUES (@UserId, @Score, @Reasons, UTC_TIMESTAMP())
            ON DUPLICATE KEY UPDATE
                score = @Score,
                reasons = @Reasons,
                last_calculated_at = UTC_TIMESTAMP()
            """, score);
    }

    public async Task<UserSuspicionScore?> GetByUserIdAsync(long userId)
    {
        using var conn = _db.CreateConnection();
        return await conn.QueryFirstOrDefaultAsync<UserSuspicionScore>(
            "SELECT * FROM user_suspicion_scores WHERE user_id = @UserId",
            new { UserId = userId });
    }

    public async Task<List<UserSuspicionScore>> GetHighScoresAsync(int threshold, int limit = 100)
    {
        using var conn = _db.CreateConnection();
        var result = await conn.QueryAsync<UserSuspicionScore>("""
            SELECT * FROM user_suspicion_scores
            WHERE score >= @Threshold
            ORDER BY score DESC
            LIMIT @Limit
            """, new { Threshold = threshold, Limit = limit });
        return result.ToList();
    }
}
