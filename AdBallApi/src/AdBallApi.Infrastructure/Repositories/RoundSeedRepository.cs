using Dapper;
using AdBallApi.Application.Repositories;
using AdBallApi.Domain.Entities;
using AdBallApi.Infrastructure.Data;

namespace AdBallApi.Infrastructure.Repositories;

public class RoundSeedRepository : IRoundSeedRepository
{
    private readonly IDbConnectionFactory _db;

    public RoundSeedRepository(IDbConnectionFactory db) => _db = db;

    public async Task<long> AddAsync(RoundSeed seed)
    {
        using var conn = _db.CreateConnection();
        return await conn.ExecuteScalarAsync<long>("""
            INSERT INTO round_seeds (round_id, block_hash, block_height, backup_seed, fetched_at)
            VALUES (@RoundId, @BlockHash, @BlockHeight, @BackupSeed, UTC_TIMESTAMP());
            SELECT LAST_INSERT_ID();
            """, seed);
    }

    public async Task<RoundSeed?> GetByRoundIdAsync(long roundId)
    {
        using var conn = _db.CreateConnection();
        return await conn.QueryFirstOrDefaultAsync<RoundSeed>(
            "SELECT * FROM round_seeds WHERE round_id = @RoundId",
            new { RoundId = roundId });
    }
}
