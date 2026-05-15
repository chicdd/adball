using AdBallApi.Domain.Entities;

namespace AdBallApi.Application.Repositories;

public interface ITransactionRepository
{
    Task<long> AddAsync(Transaction transaction);
    Task<List<Transaction>> GetByUserIdAsync(long userId, int limit = 50);
    Task<decimal> GetBalanceAsync(long userId);
}
