using AdBallApi.Domain.Entities;

namespace AdBallApi.Application.Repositories;

public interface IRoundSeedRepository
{
    Task<long> AddAsync(RoundSeed seed);
    Task<RoundSeed?> GetByRoundIdAsync(long roundId);
}
