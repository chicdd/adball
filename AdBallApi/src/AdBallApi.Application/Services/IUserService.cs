using AdBallApi.Application.DTOs.User;

namespace AdBallApi.Application.Services;

public interface IUserService
{
    Task<UserProfileResponse> GetProfileAsync(long userId);
    Task RegisterFcmTokenAsync(long userId, string token, string platform);
    Task UpdateFingerprintAsync(long userId, string? strong, string? weak);
}
