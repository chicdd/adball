using AdBallApi.Domain.Entities;

namespace AdBallApi.Application.Repositories;

public interface IUserSuspicionScoreRepository
{
    Task UpsertAsync(UserSuspicionScore score);
    Task<UserSuspicionScore?> GetByUserIdAsync(long userId);
    Task<List<UserSuspicionScore>> GetHighScoresAsync(int threshold, int limit = 100);
}
