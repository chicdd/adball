using AdBallApi.Application.DTOs.Ad;

namespace AdBallApi.Application.Services;

public interface IAdService
{
    Task<bool> ProcessSsvCallbackAsync(SsvCallbackRequest request, string ipAddress);
    Task<AdStatusResponse> GetTodayAdStatusAsync(long userId);
}
