using AdBallApi.Domain.Entities;

namespace AdBallApi.Application.Repositories;

public interface IRoundRepository
{
    Task<Round?> GetCurrentRoundAsync();
    Task<Round?> GetByIdAsync(long roundId);
    Task<List<Round>> GetClosedRoundsAsync(int limit = 10);
    Task<long> CreateAsync(Round round);
    Task UpdateStatusAsync(long roundId, Domain.Enums.RoundStatus status, DateTime? drawAt = null);
    Task AddAdRevenueAsync(long roundId, decimal amount);
}
