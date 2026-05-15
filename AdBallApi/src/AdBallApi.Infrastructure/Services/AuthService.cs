using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using AdBallApi.Application.DTOs.Auth;
using AdBallApi.Application.Repositories;
using AdBallApi.Application.Services;
using AdBallApi.Domain.Entities;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace AdBallApi.Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _users;
    private readonly IDistributedCache _cache;
    private readonly ISmsService _sms;
    private readonly IConfiguration _cfg;

    public AuthService(IUserRepository users, IDistributedCache cache, ISmsService sms, IConfiguration cfg)
    {
        _users = users;
        _cache = cache;
        _sms = sms;
        _cfg = cfg;
    }

    public async Task<bool> SendOtpAsync(string phoneNumber, string ipAddress)
    {
        var otp = Random.Shared.Next(100000, 999999).ToString();
        var key = $"otp:{phoneNumber}";
        await _cache.SetStringAsync(key, otp, new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
        });
        return await _sms.SendAsync(phoneNumber, $"[애드볼] 인증번호: {otp}");
    }

    public async Task<AuthResponse> VerifyOtpAndLoginAsync(VerifyOtpRequest request, string deviceFingerprint, string ipAddress)
    {
        var key = $"otp:{request.PhoneNumber}";
        var savedOtp = await _cache.GetStringAsync(key);
        if (savedOtp is null || savedOtp != request.OtpCode)
            throw new UnauthorizedAccessException("OTP가 올바르지 않거나 만료되었습니다.");

        await _cache.RemoveAsync(key);

        var phoneHash = HashPhone(request.PhoneNumber);
        var user = await _users.GetByPhoneHashAsync(phoneHash);
        bool isNew = user is null;

        if (isNew)
        {
            user = new User
            {
                PhoneHash = phoneHash,
                ReferralCode = GenerateReferralCode(),
                FingerprintStrong = deviceFingerprint
            };
            var id = await _users.CreateAsync(user);
            user.UserId = id;
        }
        else
        {
            await _users.UpdateLastLoginAsync(user!.UserId);
        }

        var (access, refresh, expiresAt) = GenerateTokens(user!);
        var refreshKey = $"refresh:{refresh}";
        await _cache.SetStringAsync(refreshKey, user.UserId.ToString(), new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromDays(int.Parse(_cfg["Jwt:RefreshTokenDays"] ?? "30"))
        });

        return new AuthResponse(access, refresh, expiresAt, isNew);
    }

    public async Task<AuthResponse> RefreshTokenAsync(string refreshToken)
    {
        var key = $"refresh:{refreshToken}";
        var userIdStr = await _cache.GetStringAsync(key);
        if (userIdStr is null)
            throw new UnauthorizedAccessException("Refresh token이 유효하지 않습니다.");

        var userId = long.Parse(userIdStr);
        var user = await _users.GetByIdAsync(userId)
            ?? throw new UnauthorizedAccessException("사용자를 찾을 수 없습니다.");

        await _cache.RemoveAsync(key);
        var (access, newRefresh, expiresAt) = GenerateTokens(user);
        var newKey = $"refresh:{newRefresh}";
        await _cache.SetStringAsync(newKey, userId.ToString(), new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromDays(int.Parse(_cfg["Jwt:RefreshTokenDays"] ?? "30"))
        });

        return new AuthResponse(access, newRefresh, expiresAt, false);
    }

    public async Task RevokeRefreshTokenAsync(string refreshToken)
    {
        await _cache.RemoveAsync($"refresh:{refreshToken}");
    }

    private (string access, string refresh, DateTime expiresAt) GenerateTokens(User user)
    {
        var secretKey = _cfg["Jwt:SecretKey"]!;
        var issuer = _cfg["Jwt:Issuer"]!;
        var audience = _cfg["Jwt:Audience"]!;
        var minutes = int.Parse(_cfg["Jwt:AccessTokenMinutes"] ?? "60");

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expiresAt = DateTime.UtcNow.AddMinutes(minutes);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.UserId.ToString()),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var token = new JwtSecurityToken(issuer, audience, claims,
            expires: expiresAt, signingCredentials: creds);
        var access = new JwtSecurityTokenHandler().WriteToken(token);
        var refresh = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));

        return (access, refresh, expiresAt);
    }

    private static string HashPhone(string phone)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(phone));
        return Convert.ToHexString(bytes).ToLower();
    }

    private static string GenerateReferralCode()
    {
        const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
        return new string(Enumerable.Range(0, 8).Select(_ => chars[Random.Shared.Next(chars.Length)]).ToArray());
    }
}
