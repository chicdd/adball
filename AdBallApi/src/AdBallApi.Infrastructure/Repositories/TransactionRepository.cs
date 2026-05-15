using Dapper;
using AdBallApi.Application.Repositories;
using AdBallApi.Domain.Entities;
using AdBallApi.Infrastructure.Data;

namespace AdBallApi.Infrastructure.Repositories;

public class TransactionRepository : ITransactionRepository
{
    private readonly IDbConnectionFactory _db;

    public TransactionRepository(IDbConnectionFactory db) => _db = db;

    public async Task<long> AddAsync(Transaction transaction)
    {
        using var conn = _db.CreateConnection();
        return await conn.ExecuteScalarAsync<long>("""
            INSERT INTO transactions (user_id, type, amount, balance_after, ref_id, note, created_at)
            VALUES (@UserId, @Type, @Amount, @BalanceAfter, @RefId, @Note, UTC_TIMESTAMP());
            SELECT LAST_INSERT_ID();
            """, transaction);
    }

    public async Task<List<Transaction>> GetByUserIdAsync(long userId, int limit = 50)
    {
        using var conn = _db.CreateConnection();
        var result = await conn.QueryAsync<Transaction>("""
            SELECT * FROM transactions
            WHERE user_id = @UserId
            ORDER BY created_at DESC
            LIMIT @Limit
            """, new { UserId = userId, Limit = limit });
        return result.ToList();
    }

    public async Task<decimal> GetBalanceAsync(long userId)
    {
        using var conn = _db.CreateConnection();
        return await conn.ExecuteScalarAsync<decimal>("""
            SELECT COALESCE(SUM(amount), 0) FROM transactions WHERE user_id = @UserId
            """, new { UserId = userId });
    }
}
