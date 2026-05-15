namespace AdBallApi.Domain.Entities;

public class BlockedIp
{
    public long BlockId { get; set; }
    public string IpAddress { get; set; } = "";
    public string Reason { get; set; } = "";
    public bool IsAuto { get; set; }
    public DateTime BlockedAt { get; set; }
}
