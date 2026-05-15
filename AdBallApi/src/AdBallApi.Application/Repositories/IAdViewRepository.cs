using AdBallApi.Domain.Entities;

namespace AdBallApi.Application.Repositories;

public interface IAdViewRepository
{
    Task<int> GetTodayCountAsync(long userId);
    Task<bool> ExistsByTransactionIdAsync(string transactionId);
    Task<long> AddAsync(AdView adView);
}
