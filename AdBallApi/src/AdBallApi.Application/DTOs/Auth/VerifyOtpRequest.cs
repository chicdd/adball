namespace AdBallApi.Application.DTOs.Auth;

public record VerifyOtpRequest(string PhoneNumber, string OtpCode, string? ReferralCode);
