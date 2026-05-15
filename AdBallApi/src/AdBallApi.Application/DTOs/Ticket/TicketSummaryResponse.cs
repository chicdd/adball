using AdBallApi.Domain.Enums;

namespace AdBallApi.Application.DTOs.Ticket;

public record TicketSummaryResponse(
    long RoundId,
    int Count,
    List<TicketItem> Tickets
);

public record TicketItem(
    long TicketId,
    TicketSource Source,
    DateTime AcquiredAt
);
