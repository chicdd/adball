namespace AdBallApi.Domain.Entities;

public class IpLog
{
    public long LogId { get; set; }
    public long? UserId { get; set; }
    public string IpAddress { get; set; } = "";
    public string Action { get; set; } = "";
    public string? CountryCode { get; set; }
    public DateTime CreatedAt { get; set; }
}
