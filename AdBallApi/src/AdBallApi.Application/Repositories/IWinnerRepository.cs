using AdBallApi.Domain.Entities;

namespace AdBallApi.Application.Repositories;

public interface IWinnerRepository
{
    Task AddRangeAsync(IEnumerable<Winner> winners);
    Task<List<Winner>> GetByRoundIdAsync(long roundId);
    Task<Winner?> GetByUserAndRoundAsync(long userId, long roundId);
    Task UpdateStatusAsync(long winnerId, Domain.Enums.WinnerStatus status);
}
