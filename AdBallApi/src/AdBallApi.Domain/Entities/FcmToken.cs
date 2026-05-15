namespace AdBallApi.Domain.Entities;

public class FcmToken
{
    public long TokenId { get; set; }
    public long UserId { get; set; }
    public string Token { get; set; } = "";
    public string Platform { get; set; } = "";
    public DateTime UpdatedAt { get; set; }
}
