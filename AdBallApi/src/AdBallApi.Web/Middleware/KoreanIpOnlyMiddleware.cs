using System.Net.Http.Json;
using Microsoft.Extensions.Caching.Distributed;

namespace AdBallApi.Web.Middleware;

public class KoreanIpOnlyMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IDistributedCache _cache;
    private readonly string _ipInfoToken;
    private readonly ILogger<KoreanIpOnlyMiddleware> _logger;

    // SSV 콜백은 AdMob 서버에서 오므로 IP 제한 제외
    private static readonly string[] SkipPaths = ["/api/ad/ssv", "/hangfire"];

    public KoreanIpOnlyMiddleware(
        RequestDelegate next,
        IDistributedCache cache,
        IConfiguration config,
        ILogger<KoreanIpOnlyMiddleware> logger)
    {
        _next = next;
        _cache = cache;
        _ipInfoToken = config["IPInfo:Token"] ?? "";
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var path = context.Request.Path.Value ?? "";
        if (Array.Exists(SkipPaths, p => path.StartsWith(p, StringComparison.OrdinalIgnoreCase)))
        {
            await _next(context);
            return;
        }

        var ip = context.Connection.RemoteIpAddress?.ToString() ?? "";

        // 로컬 개발 환경 허용
        if (ip is "127.0.0.1" or "::1" or "")
        {
            await _next(context);
            return;
        }

        var country = await GetCountryAsync(ip);
        if (country != "KR")
        {
            _logger.LogWarning("KR 외 접근 차단. IP={IP}, Country={Country}", ip, country);
            context.Response.StatusCode = 403;
            await context.Response.WriteAsJsonAsync(new { message = "한국에서만 이용 가능한 서비스입니다." });
            return;
        }

        await _next(context);
    }

    private async Task<string> GetCountryAsync(string ip)
    {
        var cacheKey = $"ip:country:{ip}";
        var cached = await _cache.GetStringAsync(cacheKey);
        if (cached is not null) return cached;

        try
        {
            using var http = new HttpClient();
            var url = string.IsNullOrEmpty(_ipInfoToken)
                ? $"https://ipinfo.io/{ip}/json"
                : $"https://ipinfo.io/{ip}/json?token={_ipInfoToken}";

            var info = await http.GetFromJsonAsync<IpInfoResponse>(url);
            var country = info?.Country ?? "UNKNOWN";

            await _cache.SetStringAsync(cacheKey, country,
                new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(24) });

            return country;
        }
        catch
        {
            return "KR"; // 조회 실패 시 허용 (서비스 가용성 우선)
        }
    }

    private sealed record IpInfoResponse(string Country);
}

public static class KoreanIpOnlyMiddlewareExtensions
{
    public static IApplicationBuilder UseKoreanIpOnly(this IApplicationBuilder app) =>
        app.UseMiddleware<KoreanIpOnlyMiddleware>();
}
