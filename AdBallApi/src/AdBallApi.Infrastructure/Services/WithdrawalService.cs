using AdBallApi.Application.DTOs.Withdrawal;
using AdBallApi.Application.Repositories;
using AdBallApi.Application.Services;
using AdBallApi.Domain.Entities;
using AdBallApi.Domain.Enums;

namespace AdBallApi.Infrastructure.Services;

public class WithdrawalService : IWithdrawalService
{
    private const decimal TaxRate = 0.088m;
    private const decimal TaxThreshold = 50_000m;
    private const decimal MonthlyLimit = 500_000m;

    private readonly IWithdrawalRepository _withdrawals;
    private readonly ITransactionRepository _transactions;

    public WithdrawalService(IWithdrawalRepository withdrawals, ITransactionRepository transactions)
    {
        _withdrawals = withdrawals;
        _transactions = transactions;
    }

    public async Task<WithdrawalResponse> RequestWithdrawalAsync(long userId, WithdrawalRequest request)
    {
        if (request.Amount <= 0)
            throw new ArgumentException("출금 금액이 올바르지 않습니다.");

        var balance = await _transactions.GetBalanceAsync(userId);
        if (balance < request.Amount)
            throw new InvalidOperationException("잔액이 부족합니다.");

        var monthlyTotal = await _withdrawals.GetTotalMonthlyAmountAsync(userId);
        if (monthlyTotal + request.Amount > MonthlyLimit)
            throw new InvalidOperationException($"월 출금 한도({MonthlyLimit:N0}원)를 초과합니다.");

        var tax = request.Amount >= TaxThreshold ? Math.Round(request.Amount * TaxRate, 0) : 0m;
        var net = request.Amount - tax;

        var withdrawal = new Withdrawal
        {
            UserId = userId,
            Amount = request.Amount,
            TaxAmount = tax,
            NetAmount = net,
            BankCode = request.BankCode,
            AccountNumber = request.AccountNumber,
            AccountHolder = request.AccountHolder
        };

        var id = await _withdrawals.CreateAsync(withdrawal);
        withdrawal.WithdrawalId = id;

        await _transactions.AddAsync(new Transaction
        {
            UserId = userId,
            Type = TransactionType.WithdrawalRequested,
            Amount = -request.Amount,
            BalanceAfter = balance - request.Amount,
            RefId = id.ToString()
        });

        return new WithdrawalResponse(id, request.Amount, tax, net, WithdrawalStatus.Pending, DateTime.UtcNow);
    }

    public async Task<List<WithdrawalResponse>> GetMyWithdrawalsAsync(long userId)
    {
        var list = await _withdrawals.GetByUserIdAsync(userId);
        return list.Select(w => new WithdrawalResponse(
            w.WithdrawalId, w.Amount, w.TaxAmount, w.NetAmount, w.Status, w.RequestedAt
        )).ToList();
    }
}
