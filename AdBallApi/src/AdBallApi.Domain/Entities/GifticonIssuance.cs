namespace AdBallApi.Domain.Entities;

public class GifticonIssuance
{
    public long IssuanceId { get; set; }
    public long WinnerId { get; set; }
    public string ProductCode { get; set; } = "";
    public string ProductName { get; set; } = "";
    public string? PinNumber { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public bool IsSent { get; set; }
    public DateTime CreatedAt { get; set; }
}
