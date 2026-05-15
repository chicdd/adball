using AdBallApi.Application.DTOs.User;
using AdBallApi.Application.Repositories;
using AdBallApi.Application.Services;
using AdBallApi.Domain.Entities;

namespace AdBallApi.Infrastructure.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _users;
    private readonly IFcmTokenRepository _fcmTokens;
    private readonly ITicketService _tickets;
    private readonly ITransactionRepository _transactions;

    public UserService(
        IUserRepository users,
        IFcmTokenRepository fcmTokens,
        ITicketService tickets,
        ITransactionRepository transactions)
    {
        _users = users;
        _fcmTokens = fcmTokens;
        _tickets = tickets;
        _transactions = transactions;
    }

    public async Task<UserProfileResponse> GetProfileAsync(long userId)
    {
        var user = await _users.GetByIdAsync(userId)
            ?? throw new KeyNotFoundException("사용자를 찾을 수 없습니다.");

        var ticketCount = await _tickets.GetMyTicketCountAsync(userId);
        var balance = await _transactions.GetBalanceAsync(userId);

        return new UserProfileResponse(
            UserId: userId,
            ReferralCode: user.ReferralCode,
            ReferralLink: $"https://adball.app/join?ref={user.ReferralCode}",
            CurrentRoundTickets: ticketCount,
            WinBalance: balance,
            CreatedAt: user.CreatedAt
        );
    }

    public async Task RegisterFcmTokenAsync(long userId, string token, string platform)
    {
        await _fcmTokens.UpsertAsync(new FcmToken
        {
            UserId = userId,
            Token = token,
            Platform = platform
        });
    }

    public async Task UpdateFingerprintAsync(long userId, string? strong, string? weak)
    {
        await _users.UpdateFingerprintAsync(userId, strong, weak);
    }
}
