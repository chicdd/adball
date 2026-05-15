using AdBallApi.Domain.Entities;

namespace AdBallApi.Application.Repositories;

public interface ITicketRepository
{
    Task<int> GetCountByRoundAndUserAsync(long roundId, long userId);
    Task<long> AddAsync(Ticket ticket);
    Task<List<Ticket>> GetByRoundAndUserAsync(long roundId, long userId);
    Task<List<long>> GetAllUserIdsByRoundAsync(long roundId);
    Task<int> GetTotalCountByRoundAsync(long roundId);
}
