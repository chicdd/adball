using AdBallApi.Application.DTOs.Withdrawal;

namespace AdBallApi.Application.Services;

public interface IWithdrawalService
{
    Task<WithdrawalResponse> RequestWithdrawalAsync(long userId, WithdrawalRequest request);
    Task<List<WithdrawalResponse>> GetMyWithdrawalsAsync(long userId);
}
