using AdBallApi.Application.DTOs.Ticket;
using AdBallApi.Application.Repositories;
using AdBallApi.Application.Services;
using AdBallApi.Domain.Entities;
using AdBallApi.Domain.Enums;

namespace AdBallApi.Infrastructure.Services;

public class TicketService : ITicketService
{
    private readonly ITicketRepository _tickets;
    private readonly IRoundRepository _rounds;

    public TicketService(ITicketRepository tickets, IRoundRepository rounds)
    {
        _tickets = tickets;
        _rounds = rounds;
    }

    public async Task<TicketSummaryResponse> GetMyTicketsAsync(long userId)
    {
        var round = await _rounds.GetCurrentRoundAsync();
        if (round is null)
            return new TicketSummaryResponse(0, 0, []);

        var tickets = await _tickets.GetByRoundAndUserAsync(round.RoundId, userId);
        var items = tickets.Select(t => new TicketItem(t.TicketId, t.Source, t.AcquiredAt)).ToList();
        return new TicketSummaryResponse(round.RoundId, items.Count, items);
    }

    public async Task<int> GetMyTicketCountAsync(long userId)
    {
        var round = await _rounds.GetCurrentRoundAsync();
        if (round is null) return 0;
        return await _tickets.GetCountByRoundAndUserAsync(round.RoundId, userId);
    }

    public async Task GrantTicketAsync(long userId, long roundId, TicketSource source, string? sourceRef = null)
    {
        var ticket = new Ticket
        {
            RoundId = roundId,
            UserId = userId,
            Source = source,
            SourceRef = sourceRef,
            AcquiredAt = DateTime.UtcNow
        };
        await _tickets.AddAsync(ticket);
    }
}
