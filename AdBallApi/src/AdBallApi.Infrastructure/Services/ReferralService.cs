using AdBallApi.Application.DTOs.Referral;
using AdBallApi.Application.Repositories;
using AdBallApi.Application.Services;
using AdBallApi.Domain.Entities;
using AdBallApi.Domain.Enums;
using Microsoft.Extensions.Configuration;

namespace AdBallApi.Infrastructure.Services;

public class ReferralService : IReferralService
{
    private readonly IUserRepository _users;
    private readonly IReferralRepository _referrals;
    private readonly ITicketService _tickets;
    private readonly IAdViewRepository _adViews;
    private readonly IRoundRepository _rounds;
    private readonly IConfiguration _cfg;

    public ReferralService(
        IUserRepository users,
        IReferralRepository referrals,
        ITicketService tickets,
        IAdViewRepository adViews,
        IRoundRepository rounds,
        IConfiguration cfg)
    {
        _users = users;
        _referrals = referrals;
        _tickets = tickets;
        _adViews = adViews;
        _rounds = rounds;
        _cfg = cfg;
    }

    public async Task<bool> ApplyReferralCodeAsync(long refereeId, string referralCode, string ipAddress)
    {
        var existing = await _referrals.GetByRefereeIdAsync(refereeId);
        if (existing is not null) return false;

        var referrer = await _users.GetByReferralCodeAsync(referralCode);
        if (referrer is null || referrer.UserId == refereeId) return false;

        var recentCount = await _referrals.GetReferralCountLast24hAsync(referrer.UserId);
        if (recentCount >= 5) return false;

        var referral = new Referral { ReferrerId = referrer.UserId, RefereeId = refereeId };
        await _referrals.CreateAsync(referral);

        var round = await _rounds.GetCurrentRoundAsync();
        if (round is not null)
            await _tickets.GrantTicketAsync(refereeId, round.RoundId, TicketSource.ReferralJoin, referralCode);

        return true;
    }

    public async Task<ReferralStatusResponse> GetMyReferralsAsync(long userId)
    {
        var user = await _users.GetByIdAsync(userId);
        if (user is null) throw new KeyNotFoundException();

        var referrals = await _referrals.GetByReferrerIdAsync(userId);
        var bonusEarned = referrals.Count(r => r.BonusGrantedAt.HasValue);
        var pending = referrals.Count(r => !r.BonusGrantedAt.HasValue);
        var appBaseUrl = _cfg["App:BaseUrl"] ?? "https://adball.app";

        return new ReferralStatusResponse(
            MyCode: user.ReferralCode,
            MyReferralLink: $"{appBaseUrl}/join?ref={user.ReferralCode}",
            TotalReferrals: referrals.Count,
            PendingBonus: pending,
            BonusEarned: bonusEarned
        );
    }

    public async Task ProcessReferralBonusIfEligibleAsync(long refereeId)
    {
        var referral = await _referrals.GetByRefereeIdAsync(refereeId);
        if (referral is null || referral.BonusGrantedAt.HasValue) return;

        var todayCount = await _adViews.GetTodayCountAsync(refereeId);
        if (todayCount < 5) return;

        var round = await _rounds.GetCurrentRoundAsync();
        if (round is null) return;

        await _tickets.GrantTicketAsync(referral.ReferrerId, round.RoundId, TicketSource.ReferralActivity, refereeId.ToString());
        await _referrals.UpdateBonusGrantedAsync(referral.ReferralId, DateTime.UtcNow);
    }
}
