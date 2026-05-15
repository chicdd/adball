using Dapper;
using AdBallApi.Application.Repositories;
using AdBallApi.Domain.Entities;
using AdBallApi.Infrastructure.Data;

namespace AdBallApi.Infrastructure.Repositories;

public class FcmTokenRepository : IFcmTokenRepository
{
    private readonly IDbConnectionFactory _db;

    public FcmTokenRepository(IDbConnectionFactory db) => _db = db;

    public async Task UpsertAsync(FcmToken token)
    {
        using var conn = _db.CreateConnection();
        await conn.ExecuteAsync("""
            INSERT INTO fcm_tokens (user_id, token, platform, updated_at)
            VALUES (@UserId, @Token, @Platform, UTC_TIMESTAMP())
            ON DUPLICATE KEY UPDATE
                user_id = @UserId,
                platform = @Platform,
                updated_at = UTC_TIMESTAMP()
            """, token);
    }

    public async Task<List<FcmToken>> GetByUserIdAsync(long userId)
    {
        using var conn = _db.CreateConnection();
        var result = await conn.QueryAsync<FcmToken>(
            "SELECT * FROM fcm_tokens WHERE user_id = @UserId",
            new { UserId = userId });
        return result.ToList();
    }

    public async Task<List<string>> GetAllActiveTokensAsync()
    {
        using var conn = _db.CreateConnection();
        var result = await conn.QueryAsync<string>(
            "SELECT token FROM fcm_tokens WHERE updated_at >= DATE_SUB(NOW(), INTERVAL 90 DAY)");
        return result.ToList();
    }

    public async Task DeleteAsync(string token)
    {
        using var conn = _db.CreateConnection();
        await conn.ExecuteAsync("DELETE FROM fcm_tokens WHERE token = @Token", new { Token = token });
    }
}
