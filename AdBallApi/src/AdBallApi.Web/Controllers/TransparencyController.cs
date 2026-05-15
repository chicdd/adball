using AdBallApi.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace AdBallApi.Web.Controllers;

[ApiController]
[Route("api/transparency")]
public class TransparencyController : ControllerBase
{
    private readonly IDrawService _draw;

    public TransparencyController(IDrawService draw) => _draw = draw;

    [HttpGet("prize-pool")]
    public async Task<IActionResult> GetPrizePool()
    {
        try
        {
            var current = await _draw.GetCurrentRoundAsync();
            return Ok(new
            {
                current.RoundId,
                current.WeekStart,
                current.AdRevenue,
                current.PrizePool,
                PrizePoolRatio = 0.80m,
                current.DrawAt,
                PrizeTiers = new[]
                {
                    new { Rank = 1, Winners = 1, Amount = 1_000_000 },
                    new { Rank = 2, Winners = 3, Amount = 100_000 },
                    new { Rank = 3, Winners = 20, Amount = 10_000 }
                }
            });
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { message = "진행 중인 라운드가 없습니다." });
        }
    }

    [HttpGet("draw/{roundId:long}/seed")]
    public async Task<IActionResult> GetDrawSeed(long roundId)
    {
        var result = await _draw.GetDrawResultAsync(roundId);
        if (result is null) return NotFound();

        return Ok(new
        {
            result.RoundId,
            result.BlockHash,
            result.BlockHeight,
            result.DrawnAt,
            Verification = $"https://blockstream.info/block/{result.BlockHash}"
        });
    }
}
