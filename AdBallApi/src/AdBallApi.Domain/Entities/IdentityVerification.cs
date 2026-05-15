namespace AdBallApi.Domain.Entities;

public class IdentityVerification
{
    public long VerifId { get; set; }
    public long UserId { get; set; }
    public string CiHash { get; set; } = "";
    public string MaskedName { get; set; } = "";
    public DateTime VerifiedAt { get; set; }
}
