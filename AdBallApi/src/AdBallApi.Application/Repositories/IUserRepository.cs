using AdBallApi.Domain.Entities;

namespace AdBallApi.Application.Repositories;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(long userId);
    Task<User?> GetByPhoneHashAsync(string phoneHash);
    Task<User?> GetByReferralCodeAsync(string referralCode);
    Task<long> CreateAsync(User user);
    Task UpdateAsync(User user);
    Task UpdateLastLoginAsync(long userId);
    Task UpdateFingerprintAsync(long userId, string? strong, string? weak);
    Task UpdateStatusAsync(long userId, Domain.Enums.UserStatus status);
}
