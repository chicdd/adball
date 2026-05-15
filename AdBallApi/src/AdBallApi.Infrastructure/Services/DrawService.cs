using AdBallApi.Application.DTOs.Draw;
using AdBallApi.Application.Repositories;
using AdBallApi.Application.Services;
using AdBallApi.Domain.Entities;
using AdBallApi.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace AdBallApi.Infrastructure.Services;

public class DrawService : IDrawService
{
    private static readonly (int Rank, int Count, decimal Amount)[] PrizeTiers =
    [
        (1, 1, 1_000_000m),
        (2, 3, 100_000m),
        (3, 20, 10_000m)
    ];
    private const decimal TaxRate = 0.088m;

    private readonly IRoundRepository _rounds;
    private readonly ITicketRepository _tickets;
    private readonly IWinnerRepository _winners;
    private readonly IRoundSeedRepository _seeds;
    private readonly IUserRepository _users;
    private readonly IBlockchainSeedService _blockchain;
    private readonly IPushNotificationService _push;
    private readonly ILogger<DrawService> _logger;

    public DrawService(
        IRoundRepository rounds,
        ITicketRepository tickets,
        IWinnerRepository winners,
        IRoundSeedRepository seeds,
        IUserRepository users,
        IBlockchainSeedService blockchain,
        IPushNotificationService push,
        ILogger<DrawService> logger)
    {
        _rounds = rounds;
        _tickets = tickets;
        _winners = winners;
        _seeds = seeds;
        _users = users;
        _blockchain = blockchain;
        _push = push;
        _logger = logger;
    }

    public async Task ExecuteDrawAsync()
    {
        var round = await _rounds.GetCurrentRoundAsync();
        if (round is null)
        {
            _logger.LogWarning("추첨 실행: 현재 열린 라운드 없음");
            return;
        }

        _logger.LogInformation("추첨 시작. RoundId={RoundId}", round.RoundId);
        await _rounds.UpdateStatusAsync(round.RoundId, RoundStatus.Drawing, DateTime.UtcNow);

        var (blockHash, blockHeight) = await _blockchain.FetchLatestBlockAsync();
        var seed = new RoundSeed { RoundId = round.RoundId, BlockHash = blockHash, BlockHeight = blockHeight };
        await _seeds.AddAsync(seed);

        var allUserIds = await _tickets.GetAllUserIdsByRoundAsync(round.RoundId);
        var shuffled = FisherYatesShuffle(allUserIds, blockHash);

        var winners = new List<Winner>();
        int pickedIndex = 0;

        foreach (var (rank, count, prize) in PrizeTiers)
        {
            for (int i = 0; i < count && pickedIndex < shuffled.Count; i++, pickedIndex++)
            {
                var tax = prize >= 50_000m ? Math.Round(prize * TaxRate, 0) : 0m;
                winners.Add(new Winner
                {
                    RoundId = round.RoundId,
                    UserId = shuffled[pickedIndex],
                    Rank = rank,
                    PrizeAmount = prize,
                    TaxAmount = tax,
                    NetAmount = prize - tax
                });
            }
        }

        await _winners.AddRangeAsync(winners);
        await _rounds.UpdateStatusAsync(round.RoundId, RoundStatus.Closed, DateTime.UtcNow);

        var nextWeekStart = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(7);
        await _rounds.CreateAsync(new Round { WeekStart = nextWeekStart });

        await _push.SendBatchAsync(
            await GetAllTokensAsync(),
            "🎉 추첨 결과 발표!",
            $"{round.WeekStart} 라운드 추첨이 완료되었습니다. 지금 확인해보세요!",
            new Dictionary<string, string> { ["roundId"] = round.RoundId.ToString() }
        );

        _logger.LogInformation("추첨 완료. RoundId={RoundId}, 당첨자={Count}명", round.RoundId, winners.Count);
    }

    public async Task<RoundResponse> GetCurrentRoundAsync()
    {
        var round = await _rounds.GetCurrentRoundAsync();
        if (round is null) throw new KeyNotFoundException("진행 중인 라운드가 없습니다.");
        return MapRound(round);
    }

    public async Task<List<RoundResponse>> GetDrawHistoryAsync(int limit = 10)
    {
        var rounds = await _rounds.GetClosedRoundsAsync(limit);
        return rounds.Select(MapRound).ToList();
    }

    public async Task<DrawResultResponse?> GetDrawResultAsync(long roundId)
    {
        var round = await _rounds.GetByIdAsync(roundId);
        if (round is null) return null;

        var winners = await _winners.GetByRoundIdAsync(roundId);
        var seed = await _seeds.GetByRoundIdAsync(roundId);
        var totalTickets = await _tickets.GetTotalCountByRoundAsync(roundId);

        var winnerItems = new List<WinnerItem>();
        foreach (var w in winners)
        {
            var user = await _users.GetByIdAsync(w.UserId);
            winnerItems.Add(new WinnerItem(w.Rank, MaskUserId(user?.UserId ?? 0), w.PrizeAmount, w.Status));
        }

        return new DrawResultResponse(
            RoundId: roundId,
            WeekStart: round.WeekStart,
            Winners: winnerItems,
            TotalTickets: totalTickets,
            BlockHash: seed?.BlockHash ?? "",
            BlockHeight: seed?.BlockHeight ?? 0,
            DrawnAt: round.DrawAt ?? round.CreatedAt
        );
    }

    private static List<long> FisherYatesShuffle(List<long> items, string seed)
    {
        var list = new List<long>(items);
        var seedBytes = Convert.FromHexString(seed[..32]);
        var rng = new Random(BitConverter.ToInt32(seedBytes, 0));

        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = rng.Next(i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
        return list;
    }

    private static string MaskUserId(long userId) => $"user_{userId / 1000}***";

    private static RoundResponse MapRound(Round r) =>
        new(r.RoundId, r.WeekStart, r.AdRevenue, r.PrizePool, r.Status, r.DrawAt);

    private Task<IEnumerable<string>> GetAllTokensAsync() => Task.FromResult(Enumerable.Empty<string>());
}
