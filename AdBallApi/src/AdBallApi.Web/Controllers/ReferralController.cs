using AdBallApi.Application.DTOs.Referral;
using AdBallApi.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AdBallApi.Web.Controllers;

[ApiController]
[Route("api/referral")]
[Authorize]
public class ReferralController : ControllerBase
{
    private readonly IReferralService _referral;

    public ReferralController(IReferralService referral) => _referral = referral;

    [HttpPost("apply")]
    public async Task<IActionResult> ApplyReferralCode([FromBody] ApplyReferralRequest request)
    {
        var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "";
        var ok = await _referral.ApplyReferralCodeAsync(GetUserId(), request.ReferralCode, ip);

        if (!ok) return BadRequest(new { message = "추천 코드를 적용할 수 없습니다. 이미 사용했거나 유효하지 않은 코드입니다." });
        return Ok(new { message = "추천 코드가 적용되었습니다." });
    }

    [HttpGet("me")]
    public async Task<IActionResult> GetMyReferrals()
    {
        var result = await _referral.GetMyReferralsAsync(GetUserId());
        return Ok(result);
    }

    private long GetUserId() =>
        long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue("sub")
            ?? throw new UnauthorizedAccessException());
}
