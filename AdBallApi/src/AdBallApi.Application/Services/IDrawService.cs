using AdBallApi.Application.DTOs.Draw;

namespace AdBallApi.Application.Services;

public interface IDrawService
{
    Task ExecuteDrawAsync();
    Task<RoundResponse> GetCurrentRoundAsync();
    Task<List<RoundResponse>> GetDrawHistoryAsync(int limit = 10);
    Task<DrawResultResponse?> GetDrawResultAsync(long roundId);
}
