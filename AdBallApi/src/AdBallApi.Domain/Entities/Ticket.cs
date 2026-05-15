using AdBallApi.Domain.Enums;

namespace AdBallApi.Domain.Entities;

public class Ticket
{
    public long TicketId { get; set; }
    public long RoundId { get; set; }
    public long UserId { get; set; }
    public TicketSource Source { get; set; }
    public string? SourceRef { get; set; }
    public DateTime AcquiredAt { get; set; }
}
