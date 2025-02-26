using System;
using System.Linq.Expressions;
using GameCloud.Domain.Entities.Matchmaking;

namespace GameCloud.Domain.Repositories;

public interface IStoredMatchRepository
{
    Task<StoredMatch?> GetByIdAsync(Guid id);
    Task<List<StoredMatch>> GetByGameAndQueueAsync(
        Guid gameId, 
        string queueName, 
        Expression<Func<StoredMatch, bool>>? filter = null,
        int limit = 100);
    Task<StoredMatch> CreateAsync(StoredMatch match);
    Task UpdateAsync(StoredMatch match);
    Task DeleteAsync(Guid id);
}