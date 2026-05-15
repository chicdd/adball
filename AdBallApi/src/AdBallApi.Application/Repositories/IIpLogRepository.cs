using AdBallApi.Domain.Entities;

namespace AdBallApi.Application.Repositories;

public interface IIpLogRepository
{
    Task AddAsync(IpLog log);
    Task<int> GetCountByIpAndWindowAsync(string ipAddress, TimeSpan window);
    Task<bool> IsBlockedAsync(string ipAddress);
    Task BlockAsync(string ipAddress, string reason, bool isAuto);
}
