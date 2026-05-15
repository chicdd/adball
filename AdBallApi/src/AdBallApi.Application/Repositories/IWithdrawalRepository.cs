using AdBallApi.Domain.Entities;

namespace AdBallApi.Application.Repositories;

public interface IWithdrawalRepository
{
    Task<long> CreateAsync(Withdrawal withdrawal);
    Task<List<Withdrawal>> GetByUserIdAsync(long userId);
    Task<decimal> GetTotalMonthlyAmountAsync(long userId);
    Task UpdateStatusAsync(long withdrawalId, Domain.Enums.WithdrawalStatus status, string? rejectReason = null);
}
