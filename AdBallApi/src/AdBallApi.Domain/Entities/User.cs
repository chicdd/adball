using AdBallApi.Domain.Enums;

namespace AdBallApi.Domain.Entities;

public class User
{
    public long UserId { get; set; }
    public string PhoneHash { get; set; } = "";
    public string ReferralCode { get; set; } = "";
    public string? FingerprintStrong { get; set; }
    public string? FingerprintWeak { get; set; }
    public UserStatus Status { get; set; } = UserStatus.Active;
    public DateTime CreatedAt { get; set; }
    public DateTime? LastLoginAt { get; set; }
}
