using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using AdBallApi.Application.DTOs.Ad;
using AdBallApi.Application.Repositories;
using AdBallApi.Application.Services;
using AdBallApi.Domain.Entities;
using AdBallApi.Domain.Enums;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace AdBallApi.Infrastructure.Services;

public class AdService : IAdService
{
    private const int DailyLimit = 50;
    private const int CooldownSeconds = 30;

    private readonly IAdViewRepository _adViews;
    private readonly ITicketService _tickets;
    private readonly IRoundRepository _rounds;
    private readonly IDistributedCache _cache;
    private readonly IConfiguration _cfg;
    private readonly HttpClient _http;
    private readonly ILogger<AdService> _logger;

    public AdService(
        IAdViewRepository adViews,
        ITicketService tickets,
        IRoundRepository rounds,
        IDistributedCache cache,
        IConfiguration cfg,
        HttpClient http,
        ILogger<AdService> logger)
    {
        _adViews = adViews;
        _tickets = tickets;
        _rounds = rounds;
        _cache = cache;
        _cfg = cfg;
        _http = http;
        _logger = logger;
    }

    public async Task<bool> ProcessSsvCallbackAsync(SsvCallbackRequest request, string ipAddress)
    {
        if (!await VerifySsvSignatureAsync(request))
        {
            _logger.LogWarning("SSV 서명 검증 실패. TransactionId={TxId}", request.TransactionId);
            return false;
        }

        if (await _adViews.ExistsByTransactionIdAsync(request.TransactionId))
        {
            _logger.LogWarning("중복 SSV 콜백. TransactionId={TxId}", request.TransactionId);
            return false;
        }

        if (!long.TryParse(request.UserId, out var userId))
            return false;

        var todayCount = await _adViews.GetTodayCountAsync(userId);
        if (todayCount >= DailyLimit)
            return false;

        var round = await _rounds.GetCurrentRoundAsync();
        if (round is null)
            return false;

        var adView = new AdView
        {
            UserId = userId,
            TransactionId = request.TransactionId,
            AdUnitId = request.AdUnitId,
            IpAddress = ipAddress,
            RewardGranted = true,
            AbuseScore = 0
        };
        await _adViews.AddAsync(adView);

        await _tickets.GrantTicketAsync(userId, round.RoundId, TicketSource.AdView, request.TransactionId);
        await _rounds.AddAdRevenueAsync(round.RoundId, 8m);

        await _cache.SetStringAsync($"ad:lastview:{userId}", DateTime.UtcNow.ToString("O"),
            new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(CooldownSeconds) });

        return true;
    }

    public async Task<AdStatusResponse> GetTodayAdStatusAsync(long userId)
    {
        var todayCount = await _adViews.GetTodayCountAsync(userId);
        var lastViewStr = await _cache.GetStringAsync($"ad:lastview:{userId}");
        int cooldown = 0;

        if (lastViewStr is not null && DateTime.TryParse(lastViewStr, out var lastView))
        {
            var elapsed = (int)(DateTime.UtcNow - lastView).TotalSeconds;
            cooldown = Math.Max(0, CooldownSeconds - elapsed);
        }

        return new AdStatusResponse(
            TodayCount: todayCount,
            DailyLimit: DailyLimit,
            CanWatch: todayCount < DailyLimit && cooldown == 0,
            CooldownSeconds: cooldown
        );
    }

    private async Task<bool> VerifySsvSignatureAsync(SsvCallbackRequest request)
    {
        try
        {
            var keysUrl = _cfg["AdMob:SsvPublicKeysUrl"]!;
            var json = await _http.GetStringAsync(keysUrl);
            using var doc = JsonDocument.Parse(json);

            RSA? rsa = null;
            foreach (var key in doc.RootElement.GetProperty("keys").EnumerateArray())
            {
                if (key.GetProperty("keyId").GetInt64().ToString() == request.KeyId)
                {
                    rsa = RSA.Create();
                    rsa.ImportSubjectPublicKeyInfo(Convert.FromBase64String(key.GetProperty("base64").GetString()!), out _);
                    break;
                }
            }

            if (rsa is null) return false;

            var message = $"ad_unit={request.AdUnitId}&custom_data={request.CustomData}&reward_amount=1&reward_item={request.RewardType}&timestamp={request.TransactionId}&transaction_id={request.TransactionId}&user_id={request.UserId}";
            var signatureBytes = Convert.FromBase64String(request.Signature);
            return rsa.VerifyData(Encoding.UTF8.GetBytes(message), signatureBytes, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "SSV 서명 검증 중 오류 발생");
            return false;
        }
    }
}
