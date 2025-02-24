using GameCloud.Domain.Entities.Matchmaking;

namespace GameCloud.Domain.Repositories;

public interface IMatchmakingQueueRepository
{
    Task<MatchmakingQueue?> GetByGameAndNameAsync(Guid gameId, string queueName);
    Task<MatchmakingQueue?> GetByIdAsync(Guid queueId);
    Task<IEnumerable<MatchmakingQueue>> GetAllAsync();
    Task CreateAsync(MatchmakingQueue queue);
    Task UpdateAsync(MatchmakingQueue queue);
    Task DeleteAsync(MatchmakingQueue queue);
    Task<MatchmakingQueue?> GetByIdWithFunctionsAsync(Guid queueId);
    Task<List<MatchmakingQueue>> GetByIdsAsync(List<Guid> queueIds);
    Task<IPaginate<MatchmakingQueue>> GetPagedAsync(Guid gameId, string? search, int pageIndex, int pageSize);
    Task<List<MatchmakingQueue>> GetByGameIdAsync(Guid gameId);
}

public interface IMatchTicketRepository
{
    Task<MatchTicket?> GetByIdAsync(Guid ticketId);
    Task CreateAsync(MatchTicket ticket);
    Task UpdateAsync(MatchTicket ticket);
    Task DeleteAsync(MatchTicket ticket);
    Task<IPaginate<MatchTicket>> GetQueueTicketsAsync(Guid queueId, int pageIndex, int pageSize);

    Task<List<MatchTicket>> GetActiveTicketsAsync(Guid queueId);
    Task UpdateRangeAsync(List<MatchTicket> group);
    Task<List<MatchTicket>> GetMatchTicketsAsync(Guid matchId);
    Task<List<MatchTicket>> GetPlayerActiveTicketsAsync(Guid playerId);
}