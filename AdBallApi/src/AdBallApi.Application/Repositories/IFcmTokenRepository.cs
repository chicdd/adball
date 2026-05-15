using AdBallApi.Domain.Entities;

namespace AdBallApi.Application.Repositories;

public interface IFcmTokenRepository
{
    Task UpsertAsync(FcmToken token);
    Task<List<FcmToken>> GetByUserIdAsync(long userId);
    Task<List<string>> GetAllActiveTokensAsync();
    Task DeleteAsync(string token);
}
