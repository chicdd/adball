using Dapper;
using AdBallApi.Application.Repositories;
using AdBallApi.Domain.Entities;
using AdBallApi.Infrastructure.Data;

namespace AdBallApi.Infrastructure.Repositories;

public class IpLogRepository : IIpLogRepository
{
    private readonly IDbConnectionFactory _db;

    public IpLogRepository(IDbConnectionFactory db) => _db = db;

    public async Task AddAsync(IpLog log)
    {
        using var conn = _db.CreateConnection();
        await conn.ExecuteAsync("""
            INSERT INTO ip_logs (user_id, ip_address, action, country_code, created_at)
            VALUES (@UserId, @IpAddress, @Action, @CountryCode, UTC_TIMESTAMP())
            """, log);
    }

    public async Task<int> GetCountByIpAndWindowAsync(string ipAddress, TimeSpan window)
    {
        using var conn = _db.CreateConnection();
        return await conn.ExecuteScalarAsync<int>("""
            SELECT COUNT(*) FROM ip_logs
            WHERE ip_address = @IpAddress
              AND created_at >= DATE_SUB(UTC_TIMESTAMP(), INTERVAL @Seconds SECOND)
            """, new { IpAddress = ipAddress, Seconds = (int)window.TotalSeconds });
    }

    public async Task<bool> IsBlockedAsync(string ipAddress)
    {
        using var conn = _db.CreateConnection();
        var count = await conn.ExecuteScalarAsync<int>(
            "SELECT COUNT(*) FROM blocked_ips WHERE ip_address = @IpAddress",
            new { IpAddress = ipAddress });
        return count > 0;
    }

    public async Task BlockAsync(string ipAddress, string reason, bool isAuto)
    {
        using var conn = _db.CreateConnection();
        await conn.ExecuteAsync("""
            INSERT IGNORE INTO blocked_ips (ip_address, reason, is_auto, blocked_at)
            VALUES (@IpAddress, @Reason, @IsAuto, UTC_TIMESTAMP())
            """, new { IpAddress = ipAddress, Reason = reason, IsAuto = isAuto });
    }
}
