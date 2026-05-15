namespace AdBallApi.Application.DTOs.Ad;

public record SsvCallbackRequest(
    string RewardType,
    string AdUnitId,
    string CustomData,
    string KeyId,
    string TransactionId,
    string UserId,
    string Signature
);
