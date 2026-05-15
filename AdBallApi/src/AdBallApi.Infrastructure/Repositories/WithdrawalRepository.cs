using Dapper;
using AdBallApi.Application.Repositories;
using AdBallApi.Domain.Entities;
using AdBallApi.Domain.Enums;
using AdBallApi.Infrastructure.Data;

namespace AdBallApi.Infrastructure.Repositories;

public class WithdrawalRepository : IWithdrawalRepository
{
    private readonly IDbConnectionFactory _db;

    public WithdrawalRepository(IDbConnectionFactory db) => _db = db;

    public async Task<long> CreateAsync(Withdrawal withdrawal)
    {
        using var conn = _db.CreateConnection();
        return await conn.ExecuteScalarAsync<long>("""
            INSERT INTO withdrawals (user_id, amount, tax_amount, net_amount, bank_code, account_number, account_holder, status, requested_at)
            VALUES (@UserId, @Amount, @TaxAmount, @NetAmount, @BankCode, @AccountNumber, @AccountHolder, 'pending', UTC_TIMESTAMP());
            SELECT LAST_INSERT_ID();
            """, withdrawal);
    }

    public async Task<List<Withdrawal>> GetByUserIdAsync(long userId)
    {
        using var conn = _db.CreateConnection();
        var result = await conn.QueryAsync<Withdrawal>(
            "SELECT * FROM withdrawals WHERE user_id = @UserId ORDER BY requested_at DESC",
            new { UserId = userId });
        return result.ToList();
    }

    public async Task<decimal> GetTotalMonthlyAmountAsync(long userId)
    {
        using var conn = _db.CreateConnection();
        return await conn.ExecuteScalarAsync<decimal>("""
            SELECT COALESCE(SUM(amount), 0) FROM withdrawals
            WHERE user_id = @UserId
              AND status IN ('pending', 'processing', 'completed')
              AND YEAR(requested_at) = YEAR(NOW())
              AND MONTH(requested_at) = MONTH(NOW())
            """, new { UserId = userId });
    }

    public async Task UpdateStatusAsync(long withdrawalId, WithdrawalStatus status, string? rejectReason = null)
    {
        using var conn = _db.CreateConnection();
        await conn.ExecuteAsync("""
            UPDATE withdrawals
            SET status = @Status,
                reject_reason = @RejectReason,
                processed_at = CASE WHEN @Status IN ('completed','rejected') THEN UTC_TIMESTAMP() ELSE processed_at END
            WHERE withdrawal_id = @WithdrawalId
            """, new { WithdrawalId = withdrawalId, Status = status.ToString().ToLower(), RejectReason = rejectReason });
    }
}
