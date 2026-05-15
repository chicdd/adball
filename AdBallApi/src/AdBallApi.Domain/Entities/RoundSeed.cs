namespace AdBallApi.Domain.Entities;

public class RoundSeed
{
    public long SeedId { get; set; }
    public long RoundId { get; set; }
    public string BlockHash { get; set; } = "";
    public long BlockHeight { get; set; }
    public string? BackupSeed { get; set; }
    public DateTime FetchedAt { get; set; }
}
