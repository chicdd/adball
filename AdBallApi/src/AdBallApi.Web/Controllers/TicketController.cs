using AdBallApi.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AdBallApi.Web.Controllers;

[ApiController]
[Route("api/tickets")]
[Authorize]
public class TicketController : ControllerBase
{
    private readonly ITicketService _tickets;

    public TicketController(ITicketService tickets) => _tickets = tickets;

    [HttpGet("me")]
    public async Task<IActionResult> GetMyTickets()
    {
        var summary = await _tickets.GetMyTicketsAsync(GetUserId());
        return Ok(summary);
    }

    [HttpGet("count")]
    public async Task<IActionResult> GetMyTicketCount()
    {
        var count = await _tickets.GetMyTicketCountAsync(GetUserId());
        return Ok(new { count });
    }

    private long GetUserId() =>
        long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue("sub")
            ?? throw new UnauthorizedAccessException());
}
