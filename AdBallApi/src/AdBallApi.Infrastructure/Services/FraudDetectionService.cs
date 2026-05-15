using AdBallApi.Application.Repositories;
using AdBallApi.Application.Services;
using AdBallApi.Domain.Entities;
using AdBallApi.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace AdBallApi.Infrastructure.Services;

public class FraudDetectionService : IFraudDetectionService
{
    private const int AutoFlagThreshold = 70;
    private const int AutoBanThreshold = 100;

    private readonly IUserSuspicionScoreRepository _scores;
    private readonly IUserRepository _users;
    private readonly IAdViewRepository _adViews;
    private readonly IIpLogRepository _ipLogs;
    private readonly ILogger<FraudDetectionService> _logger;

    public FraudDetectionService(
        IUserSuspicionScoreRepository scores,
        IUserRepository users,
        IAdViewRepository adViews,
        IIpLogRepository ipLogs,
        ILogger<FraudDetectionService> logger)
    {
        _scores = scores;
        _users = users;
        _adViews = adViews;
        _ipLogs = ipLogs;
        _logger = logger;
    }

    public async Task RecomputeAllScoresAsync()
    {
        var highScores = await _scores.GetHighScoresAsync(0, 10000);
        _logger.LogInformation("의심 점수 재계산 시작. 대상={Count}명", highScores.Count);

        foreach (var existing in highScores)
        {
            await CheckAndFlagUserAsync(existing.UserId);
        }
    }

    public async Task<int> ComputeScoreAsync(long userId)
    {
        int score = 0;
        var reasons = new List<string>();

        var todayAdCount = await _adViews.GetTodayCountAsync(userId);
        if (todayAdCount >= 48)
        {
            score += 20;
            reasons.Add("일일 광고 한도 근접");
        }

        var recentIpCount = await _ipLogs.GetCountByIpAndWindowAsync("", TimeSpan.FromHours(1));
        if (recentIpCount > 50)
        {
            score += 30;
            reasons.Add("IP 비정상 요청 빈도");
        }

        return score;
    }

    public async Task CheckAndFlagUserAsync(long userId)
    {
        var score = await ComputeScoreAsync(userId);

        var existing = await _scores.GetByUserIdAsync(userId);
        var scoreRecord = new UserSuspicionScore
        {
            UserId = userId,
            Score = score,
            Reasons = string.Join(", ", new List<string>())
        };
        await _scores.UpsertAsync(scoreRecord);

        if (score >= AutoBanThreshold)
        {
            await _users.UpdateStatusAsync(userId, UserStatus.Banned);
            _logger.LogWarning("사용자 자동 정지. UserId={UserId}, Score={Score}", userId, score);
        }
        else if (score >= AutoFlagThreshold)
        {
            await _users.UpdateStatusAsync(userId, UserStatus.Suspended);
            _logger.LogWarning("사용자 자동 플래그. UserId={UserId}, Score={Score}", userId, score);
        }
    }
}
