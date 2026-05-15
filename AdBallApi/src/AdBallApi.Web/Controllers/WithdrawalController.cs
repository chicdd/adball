using AdBallApi.Application.DTOs.Withdrawal;
using AdBallApi.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AdBallApi.Web.Controllers;

[ApiController]
[Route("api/withdrawal")]
[Authorize]
public class WithdrawalController : ControllerBase
{
    private readonly IWithdrawalService _withdrawal;

    public WithdrawalController(IWithdrawalService withdrawal) => _withdrawal = withdrawal;

    [HttpPost]
    public async Task<IActionResult> RequestWithdrawal([FromBody] WithdrawalRequest request)
    {
        try
        {
            var result = await _withdrawal.RequestWithdrawalAsync(GetUserId(), request);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("me")]
    public async Task<IActionResult> GetMyWithdrawals()
    {
        var list = await _withdrawal.GetMyWithdrawalsAsync(GetUserId());
        return Ok(list);
    }

    private long GetUserId() =>
        long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue("sub")
            ?? throw new UnauthorizedAccessException());
}
