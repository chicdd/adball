using Dapper;
using AdBallApi.Application.Repositories;
using AdBallApi.Domain.Entities;
using AdBallApi.Infrastructure.Data;

namespace AdBallApi.Infrastructure.Repositories;

public class TicketRepository : ITicketRepository
{
    private readonly IDbConnectionFactory _db;

    public TicketRepository(IDbConnectionFactory db) => _db = db;

    public async Task<int> GetCountByRoundAndUserAsync(long roundId, long userId)
    {
        using var conn = _db.CreateConnection();
        return await conn.ExecuteScalarAsync<int>(
            "SELECT COUNT(*) FROM tickets WHERE round_id = @RoundId AND user_id = @UserId",
            new { RoundId = roundId, UserId = userId });
    }

    public async Task<long> AddAsync(Ticket ticket)
    {
        using var conn = _db.CreateConnection();
        return await conn.ExecuteScalarAsync<long>("""
            INSERT INTO tickets (round_id, user_id, source, source_ref, acquired_at)
            VALUES (@RoundId, @UserId, @Source, @SourceRef, @AcquiredAt);
            SELECT LAST_INSERT_ID();
            """, ticket);
    }

    public async Task<List<Ticket>> GetByRoundAndUserAsync(long roundId, long userId)
    {
        using var conn = _db.CreateConnection();
        var result = await conn.QueryAsync<Ticket>("""
            SELECT ticket_id, round_id, user_id, source, source_ref, acquired_at
            FROM tickets
            WHERE round_id = @RoundId AND user_id = @UserId
            ORDER BY acquired_at DESC
            """, new { RoundId = roundId, UserId = userId });
        return result.ToList();
    }

    public async Task<List<long>> GetAllUserIdsByRoundAsync(long roundId)
    {
        using var conn = _db.CreateConnection();
        var result = await conn.QueryAsync<long>(
            "SELECT user_id FROM tickets WHERE round_id = @RoundId",
            new { RoundId = roundId });
        return result.ToList();
    }

    public async Task<int> GetTotalCountByRoundAsync(long roundId)
    {
        using var conn = _db.CreateConnection();
        return await conn.ExecuteScalarAsync<int>(
            "SELECT COUNT(*) FROM tickets WHERE round_id = @RoundId",
            new { RoundId = roundId });
    }
}
