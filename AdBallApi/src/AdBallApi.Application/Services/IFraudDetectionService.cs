namespace AdBallApi.Application.Services;

public interface IFraudDetectionService
{
    Task RecomputeAllScoresAsync();
    Task<int> ComputeScoreAsync(long userId);
    Task CheckAndFlagUserAsync(long userId);
}
