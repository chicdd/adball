using AdBallApi.Application.DTOs.Ad;
using AdBallApi.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AdBallApi.Web.Controllers;

[ApiController]
[Route("api/ad")]
public class AdController : ControllerBase
{
    private readonly IAdService _ad;

    public AdController(IAdService ad) => _ad = ad;

    // AdMob SSV 서버가 호출하는 엔드포인트 (인증 없음)
    [HttpGet("ssv")]
    public async Task<IActionResult> SsvCallback([FromQuery] SsvCallbackRequest request)
    {
        var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "";
        var ok = await _ad.ProcessSsvCallbackAsync(request, ip);
        // AdMob은 200이면 성공으로 간주, 실패도 200 반환 (재시도 방지)
        return Ok(ok ? "reward_granted" : "no_reward");
    }

    [Authorize]
    [HttpGet("status")]
    public async Task<IActionResult> GetAdStatus()
    {
        var userId = GetUserId();
        var status = await _ad.GetTodayAdStatusAsync(userId);
        return Ok(status);
    }

    private long GetUserId() =>
        long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue("sub")
            ?? throw new UnauthorizedAccessException());
}
