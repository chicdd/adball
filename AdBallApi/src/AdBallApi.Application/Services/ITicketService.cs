using AdBallApi.Application.DTOs.Ticket;
using AdBallApi.Domain.Enums;

namespace AdBallApi.Application.Services;

public interface ITicketService
{
    Task<TicketSummaryResponse> GetMyTicketsAsync(long userId);
    Task<int> GetMyTicketCountAsync(long userId);
    Task GrantTicketAsync(long userId, long roundId, TicketSource source, string? sourceRef = null);
}
