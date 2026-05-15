using AdBallApi.Application.DTOs.Auth;

namespace AdBallApi.Application.Services;

public interface IAuthService
{
    Task<bool> SendOtpAsync(string phoneNumber, string ipAddress);
    Task<AuthResponse> VerifyOtpAndLoginAsync(VerifyOtpRequest request, string deviceFingerprint, string ipAddress);
    Task<AuthResponse> RefreshTokenAsync(string refreshToken);
    Task RevokeRefreshTokenAsync(string refreshToken);
}
