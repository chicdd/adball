using AdBallApi.Application.DTOs.User;
using AdBallApi.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AdBallApi.Web.Controllers;

[ApiController]
[Route("api/user")]
[Authorize]
public class UserController : ControllerBase
{
    private readonly IUserService _user;

    public UserController(IUserService user) => _user = user;

    [HttpGet("me")]
    public async Task<IActionResult> GetProfile()
    {
        try
        {
            var profile = await _user.GetProfileAsync(GetUserId());
            return Ok(profile);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    [HttpPut("fcm-token")]
    public async Task<IActionResult> RegisterFcmToken([FromBody] RegisterFcmTokenRequest request)
    {
        await _user.RegisterFcmTokenAsync(GetUserId(), request.Token, request.Platform);
        return NoContent();
    }

    [HttpPut("fingerprint")]
    public async Task<IActionResult> UpdateFingerprint([FromBody] FingerprintRequest request)
    {
        await _user.UpdateFingerprintAsync(GetUserId(), request.Strong, request.Weak);
        return NoContent();
    }

    private long GetUserId() =>
        long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue("sub")
            ?? throw new UnauthorizedAccessException());
}
