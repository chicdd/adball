using Dapper;
using AdBallApi.Application.Repositories;
using AdBallApi.Domain.Entities;
using AdBallApi.Domain.Enums;
using AdBallApi.Infrastructure.Data;

namespace AdBallApi.Infrastructure.Repositories;

public class RoundRepository : IRoundRepository
{
    private readonly IDbConnectionFactory _db;

    public RoundRepository(IDbConnectionFactory db) => _db = db;

    public async Task<Round?> GetCurrentRoundAsync()
    {
        using var conn = _db.CreateConnection();
        return await conn.QueryFirstOrDefaultAsync<Round>(
            "SELECT * FROM rounds WHERE status = 'open' ORDER BY round_id DESC LIMIT 1");
    }

    public async Task<Round?> GetByIdAsync(long roundId)
    {
        using var conn = _db.CreateConnection();
        return await conn.QueryFirstOrDefaultAsync<Round>(
            "SELECT * FROM rounds WHERE round_id = @RoundId", new { RoundId = roundId });
    }

    public async Task<List<Round>> GetClosedRoundsAsync(int limit = 10)
    {
        using var conn = _db.CreateConnection();
        var result = await conn.QueryAsync<Round>(
            "SELECT * FROM rounds WHERE status = 'closed' ORDER BY round_id DESC LIMIT @Limit",
            new { Limit = limit });
        return result.ToList();
    }

    public async Task<long> CreateAsync(Round round)
    {
        using var conn = _db.CreateConnection();
        return await conn.ExecuteScalarAsync<long>("""
            INSERT INTO rounds (week_start, ad_revenue, prize_pool, status, created_at)
            VALUES (@WeekStart, 0, 0, 'open', UTC_TIMESTAMP());
            SELECT LAST_INSERT_ID();
            """, round);
    }

    public async Task UpdateStatusAsync(long roundId, RoundStatus status, DateTime? drawAt = null)
    {
        using var conn = _db.CreateConnection();
        await conn.ExecuteAsync("""
            UPDATE rounds SET status = @Status, draw_at = @DrawAt
            WHERE round_id = @RoundId
            """, new { RoundId = roundId, Status = status.ToString().ToLower(), DrawAt = drawAt });
    }

    public async Task AddAdRevenueAsync(long roundId, decimal amount)
    {
        using var conn = _db.CreateConnection();
        await conn.ExecuteAsync("""
            UPDATE rounds
            SET ad_revenue = ad_revenue + @Amount,
                prize_pool = prize_pool + @Amount * 0.8
            WHERE round_id = @RoundId
            """, new { RoundId = roundId, Amount = amount });
    }
}
