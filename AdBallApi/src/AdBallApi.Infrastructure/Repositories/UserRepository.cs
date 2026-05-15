using Dapper;
using AdBallApi.Application.Repositories;
using AdBallApi.Domain.Entities;
using AdBallApi.Domain.Enums;
using AdBallApi.Infrastructure.Data;

namespace AdBallApi.Infrastructure.Repositories;

public class UserRepository : IUserRepository
{
    private readonly IDbConnectionFactory _db;

    public UserRepository(IDbConnectionFactory db) => _db = db;

    public async Task<User?> GetByIdAsync(long userId)
    {
        using var conn = _db.CreateConnection();
        return await conn.QueryFirstOrDefaultAsync<User>(
            "SELECT * FROM users WHERE user_id = @UserId", new { UserId = userId });
    }

    public async Task<User?> GetByPhoneHashAsync(string phoneHash)
    {
        using var conn = _db.CreateConnection();
        return await conn.QueryFirstOrDefaultAsync<User>(
            "SELECT * FROM users WHERE phone_hash = @PhoneHash", new { PhoneHash = phoneHash });
    }

    public async Task<User?> GetByReferralCodeAsync(string referralCode)
    {
        using var conn = _db.CreateConnection();
        return await conn.QueryFirstOrDefaultAsync<User>(
            "SELECT * FROM users WHERE referral_code = @Code", new { Code = referralCode });
    }

    public async Task<long> CreateAsync(User user)
    {
        using var conn = _db.CreateConnection();
        return await conn.ExecuteScalarAsync<long>("""
            INSERT INTO users (phone_hash, referral_code, fingerprint_strong, fingerprint_weak, status, created_at)
            VALUES (@PhoneHash, @ReferralCode, @FingerprintStrong, @FingerprintWeak, @Status, UTC_TIMESTAMP());
            SELECT LAST_INSERT_ID();
            """, user);
    }

    public async Task UpdateAsync(User user)
    {
        using var conn = _db.CreateConnection();
        await conn.ExecuteAsync("""
            UPDATE users SET fingerprint_strong = @FingerprintStrong,
                             fingerprint_weak = @FingerprintWeak,
                             status = @Status
            WHERE user_id = @UserId
            """, user);
    }

    public async Task UpdateLastLoginAsync(long userId)
    {
        using var conn = _db.CreateConnection();
        await conn.ExecuteAsync(
            "UPDATE users SET last_login_at = UTC_TIMESTAMP() WHERE user_id = @UserId",
            new { UserId = userId });
    }

    public async Task UpdateFingerprintAsync(long userId, string? strong, string? weak)
    {
        using var conn = _db.CreateConnection();
        await conn.ExecuteAsync("""
            UPDATE users SET fingerprint_strong = @Strong, fingerprint_weak = @Weak
            WHERE user_id = @UserId
            """, new { UserId = userId, Strong = strong, Weak = weak });
    }

    public async Task UpdateStatusAsync(long userId, UserStatus status)
    {
        using var conn = _db.CreateConnection();
        await conn.ExecuteAsync(
            "UPDATE users SET status = @Status WHERE user_id = @UserId",
            new { UserId = userId, Status = status.ToString().ToLower() });
    }
}
