using Dapper;
using AdBallApi.Application.Repositories;
using AdBallApi.Domain.Entities;
using AdBallApi.Domain.Enums;
using AdBallApi.Infrastructure.Data;

namespace AdBallApi.Infrastructure.Repositories;

public class WinnerRepository : IWinnerRepository
{
    private readonly IDbConnectionFactory _db;

    public WinnerRepository(IDbConnectionFactory db) => _db = db;

    public async Task AddRangeAsync(IEnumerable<Winner> winners)
    {
        using var conn = _db.CreateConnection();
        await conn.ExecuteAsync("""
            INSERT INTO winners (round_id, user_id, rank, prize_amount, tax_amount, net_amount, status, created_at)
            VALUES (@RoundId, @UserId, @Rank, @PrizeAmount, @TaxAmount, @NetAmount, 'pending', UTC_TIMESTAMP())
            """, winners);
    }

    public async Task<List<Winner>> GetByRoundIdAsync(long roundId)
    {
        using var conn = _db.CreateConnection();
        var result = await conn.QueryAsync<Winner>(
            "SELECT * FROM winners WHERE round_id = @RoundId ORDER BY rank",
            new { RoundId = roundId });
        return result.ToList();
    }

    public async Task<Winner?> GetByUserAndRoundAsync(long userId, long roundId)
    {
        using var conn = _db.CreateConnection();
        return await conn.QueryFirstOrDefaultAsync<Winner>(
            "SELECT * FROM winners WHERE user_id = @UserId AND round_id = @RoundId",
            new { UserId = userId, RoundId = roundId });
    }

    public async Task UpdateStatusAsync(long winnerId, WinnerStatus status)
    {
        using var conn = _db.CreateConnection();
        await conn.ExecuteAsync(
            "UPDATE winners SET status = @Status WHERE winner_id = @WinnerId",
            new { WinnerId = winnerId, Status = status.ToString().ToLower() });
    }
}
