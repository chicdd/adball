namespace AdBallApi.Domain.Entities;

public class Referral
{
    public long ReferralId { get; set; }
    public long ReferrerId { get; set; }
    public long RefereeId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? BonusGrantedAt { get; set; }
}
