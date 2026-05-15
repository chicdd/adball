using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace AdBallApi.Infrastructure.Services;

public interface IBlockchainSeedService
{
    Task<(string Hash, long Height)> FetchLatestBlockAsync();
}

public class BlockstreamSeedService : IBlockchainSeedService
{
    private readonly HttpClient _http;
    private readonly IConfiguration _cfg;
    private readonly ILogger<BlockstreamSeedService> _logger;

    public BlockstreamSeedService(HttpClient http, IConfiguration cfg, ILogger<BlockstreamSeedService> logger)
    {
        _http = http;
        _cfg = cfg;
        _logger = logger;
    }

    public async Task<(string Hash, long Height)> FetchLatestBlockAsync()
    {
        var baseUrl = _cfg["Draw:BlockstreamApiBase"] ?? "https://blockstream.info/api";

        for (int attempt = 0; attempt < 6; attempt++)
        {
            try
            {
                var hash = await _http.GetStringAsync($"{baseUrl}/blocks/tip/hash");
                hash = hash.Trim();
                var blockJson = await _http.GetStringAsync($"{baseUrl}/block/{hash}");
                using var doc = JsonDocument.Parse(blockJson);
                var height = doc.RootElement.GetProperty("height").GetInt64();
                _logger.LogInformation("비트코인 블록 조회 성공. Hash={Hash}, Height={Height}", hash, height);
                return (hash, height);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "비트코인 블록 조회 실패. 시도={Attempt}", attempt + 1);
                await Task.Delay(TimeSpan.FromMinutes(5));
            }
        }

        var fallback = DateTime.UtcNow.Ticks.ToString("x16");
        _logger.LogError("비트코인 블록 조회 최종 실패. 폴백 시드 사용: {Seed}", fallback);
        return (fallback.PadRight(64, '0'), 0);
    }
}
