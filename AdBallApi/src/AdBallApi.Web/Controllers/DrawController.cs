using AdBallApi.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace AdBallApi.Web.Controllers;

[ApiController]
[Route("api/draw")]
public class DrawController : ControllerBase
{
    private readonly IDrawService _draw;

    public DrawController(IDrawService draw) => _draw = draw;

    [HttpGet("current")]
    public async Task<IActionResult> GetCurrentRound()
    {
        try
        {
            var round = await _draw.GetCurrentRoundAsync();
            return Ok(round);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    [HttpGet("history")]
    public async Task<IActionResult> GetDrawHistory([FromQuery] int limit = 10)
    {
        var history = await _draw.GetDrawHistoryAsync(Math.Clamp(limit, 1, 50));
        return Ok(history);
    }

    [HttpGet("{roundId:long}")]
    public async Task<IActionResult> GetDrawResult(long roundId)
    {
        var result = await _draw.GetDrawResultAsync(roundId);
        if (result is null) return NotFound(new { message = "해당 라운드를 찾을 수 없습니다." });
        return Ok(result);
    }
}
